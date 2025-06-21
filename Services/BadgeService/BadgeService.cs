using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly ApplicationDbContext _context;

        public BadgeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AssignBadgeBasedOnCertificatesAsync(int userId)
        {
            var user = await _context.Users
            .Include(u => u.Certificats)
            .Include(u => u.Badge)
            .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new Exception("Utilisateur non trouvé");
            }

            await AssignBadgeToUserAsync(user);
        }

        public async Task AssignBadgesToAllUsersAsync()
        {
            // Retrieve all users with their certificates and badges
            var users = await _context.Users
            .Include(u => u.Certificats)
            .Include(u => u.Badge)
            .ToListAsync();

            foreach (var user in users)
            {
                await AssignBadgeToUserAsync(user);
            }

            // Save all changes in a single transaction
            await _context.SaveChangesAsync();
        }

        private async Task AssignBadgeToUserAsync(User user)
        {
            // Count certificates
            int certificateCount = user.Certificats?.Count ?? 0;

            // Determine badge based on certificate count
            BadgeName badgeName = certificateCount switch
            {
                0 => BadgeName.Beginner,
                1 => BadgeName.Amateur,
                _ => BadgeName.Pro
            };

            // Find badge in database
            var badge = await _context.Badges.FirstOrDefaultAsync(b => b.BadgeName == badgeName);

            if (badge == null)
            {
                // Create badge if it doesn't exist
                badge = new Badge { BadgeName = badgeName };
                _context.Badges.Add(badge);
                await _context.SaveChangesAsync();
            }

            // Assign badge to user
            user.BadgeId = badge.BadgeId;
            user.Badge = badge;

            // Mark user as modified
            _context.Users.Update(user);
        }
    }
}