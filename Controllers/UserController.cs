﻿using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMoodleService _moodleService;

        public UserController(IUserService userService, IMoodleService moodleService)
        {
            _userService = userService;
            _moodleService = moodleService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserRequest request)
        {
            if (request.CvFile == null || request.CvFile.Length == 0)
            {
                return BadRequest("CV file is required.");
            }

            var result = await _userService.RegisterUser(
                request.Username,
                request.Firstname,
                request.Lastname,
                request.Password,
                 request.confirmPassword,
                request.Email,
                request.CvFile
               
             
            );

            if (!result)
            {
                return BadRequest("User registration failed.");
            }

            return Ok("User registered successfully.");
        }

        public class RegisterUserRequest
        {
            public string Username { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public IFormFile CvFile { get; set; }
            public RoleName Role { get; set; }
            public string confirmPassword { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                return Ok(users); // Return the array directly
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                // Call the service to delete the user
                var result = await _userService.DeleteUserAsync(userId);

                if (result)
                {
                    return Ok(new { message = "User deleted successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("id/{id:int}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("completion/{userId}/{courseId}")]
        public async Task<IActionResult> GetUserCourseCompletionStatus(int userId, int courseId)
        {
            try
            {
                var completionStatus = await _userService.GetUserCourseCompletionStatusAsync(userId, courseId);
                return Ok(completionStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("grades/{courseId}/{userId}")]
        public async Task<ActionResult<List<MoodleGradeItem>>> GetUserGrades(int courseId, int userId)
        {
            try
            {


                var grades = await _moodleService.GetUserGradesAsync(courseId, userId);

                if (grades == null || grades.Count == 0)
                {
                    return NotFound("No grades found for this user in the specified course");
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }
    }
}