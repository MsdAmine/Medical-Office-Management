using MedicalOfficeManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly MedicalOfficeDbContext _context;

        public PatientRepository(MedicalOfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PatientEntity>> ListAsync(CancellationToken cancellationToken)
        {
            return await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PatientEntity>> ListWithAppointmentsAsync(CancellationToken cancellationToken)
        {
            return await _context.Patients
                .AsNoTracking()
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<PatientEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Patients
                .AsNoTracking()
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }

    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly MedicalOfficeDbContext _context;

        public AppointmentRepository(MedicalOfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AppointmentEntity>> ListAsync(CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderBy(a => a.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AppointmentEntity>> ListRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.StartTime >= start && a.StartTime <= end)
                .OrderBy(a => a.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AppointmentEntity>> ListForDateAsync(DateTime date, CancellationToken cancellationToken)
        {
            var nextDay = date.Date.AddDays(1);
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.StartTime >= date.Date && a.StartTime < nextDay)
                .OrderBy(a => a.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<AppointmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }
    }

    public class DoctorRepository : IDoctorRepository
    {
        private readonly MedicalOfficeDbContext _context;

        public DoctorRepository(MedicalOfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DoctorEntity>> ListAsync(CancellationToken cancellationToken)
        {
            return await _context.Doctors
                .AsNoTracking()
                .OrderBy(d => d.FullName)
                .ToListAsync(cancellationToken);
        }

        public async Task<DoctorEntity?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.Doctors
                .AsNoTracking()
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }
    }
}
