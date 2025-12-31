(function () {
    const toast = document.getElementById('realtime-toast');
    const toastMessage = document.getElementById('realtime-toast-message');
    let toastTimeout;

    const containers = {
        dashboard: document.getElementById('dashboard-live-section'),
        appointments: document.getElementById('appointments-live-container'),
        doctors: document.getElementById('doctors-live-section')
    };

    function showToast(message) {
        if (!toast || !toastMessage) return;
        toastMessage.textContent = message;
        toast.classList.remove('hidden');

        if (toastTimeout) {
            clearTimeout(toastTimeout);
        }
        toastTimeout = setTimeout(() => toast.classList.add('hidden'), 3200);
    }

    function normalizeViews(views) {
        if (!views || !Array.isArray(views)) return [];
        return views.map(v => (v || '').toString().toLowerCase());
    }

    async function refreshContainer(container) {
        if (!container) return;
        const url = container.dataset.partialUrl;
        if (!url) return;

        try {
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), 8000);
            const response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                signal: controller.signal
            });
            clearTimeout(timeout);
            if (!response.ok) return;
            const html = await response.text();
            container.innerHTML = html;
        } catch (error) {
            console.warn('Soft refresh failed', error);
        }
    }

    function resolveViews(payload, fallback) {
        const declared = normalizeViews(payload?.affectedViews);
        if (declared.length) return declared;
        return fallback;
    }

    function handleEvent(payload, message, fallbackViews = []) {
        const views = resolveViews(payload, fallbackViews);
        const viewKey = (document.body?.dataset?.view || '').toLowerCase();

        if (views.includes('dashboard') && containers.dashboard && viewKey === 'home') {
            refreshContainer(containers.dashboard);
        }
        if (views.includes('appointments') && containers.appointments && viewKey === 'appointments') {
            refreshContainer(containers.appointments);
        }
        if (views.includes('doctors') && containers.doctors && viewKey === 'doctors') {
            refreshContainer(containers.doctors);
        }

        if (views.includes(viewKey) || views.length === 0) {
            showToast(message);
        }
    }

    if (!window.signalR) {
        console.warn('SignalR client library is missing');
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/clinic')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.None)
        .build();

    connection.on('AppointmentCreated', payload => handleEvent(payload, 'Appointments updated', ['appointments', 'dashboard']));
    connection.on('AppointmentStatusChanged', payload => handleEvent(payload, 'Appointment status changed', ['appointments', 'dashboard']));
    connection.on('AppointmentCancelled', payload => handleEvent(payload, 'Appointment cancelled', ['appointments', 'dashboard']));
    connection.on('PatientCheckedIn', payload => handleEvent(payload, 'Patient check-in recorded', ['appointments', 'dashboard']));
    connection.on('InvoiceStatusUpdated', payload => handleEvent(payload, 'Billing status updated', ['billing']));
    connection.on('DoctorAvailabilityChanged', payload => handleEvent(payload, 'Doctor availability updated', ['doctors', 'dashboard']));

    connection.onreconnected(() => showToast('Reconnected to live updates'));
    connection.onclose(() => showToast('Live updates paused. Retrying...'));

    connection.start()
        .then(() => {
            showToast('Live updates connected');
        })
        .catch(err => console.warn('SignalR connection failed', err));
})();
