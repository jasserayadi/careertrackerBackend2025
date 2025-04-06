using Career_Tracker_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.CertificateService
{
    public interface ICertificateService
    {
        Task<bool> IsCertificateEligible(int userId, int courseId);
        Task<Certificat> GenerateAndSaveCertificate(int userId, int courseId, string courseName);
        Task<List<Certificat>> GetUserCertificates(int userId);
        Task<Certificat?> GetCertificateById(int certificateId);
        Task<Certificat?> GetCertificateForCourse(int userId, int courseId);
        Task<Certificat?> VerifyCertificate(string verificationCode);
    }
}