﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using System.Threading.Tasks;
using AppCore.DTOs;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using System.Linq;
using AppCore.Entities;
using AppCore.HelperEntities;
using Infrastructure.Interfaces;
using AppCore.Interfaces;
using AppCore.Exceptions;

namespace date_app.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController: ControllerBase
    {
        private readonly IPhotoService _photoService;
        //private readonly IUserRepository _userRepo;
        //private readonly IMapper _mapper;
        //private readonly IOptions<CloudinarySettings> _cloudinarySettings;
        //private Cloudinary _cloudinary;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
            //_mapper = mapper;           
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _photoService.GetPhoto(id);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
            [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            try
            {
                var photo = await _photoService.AddForUser(userId, photoForCreationDto);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photo);
            }
            catch (SaveDataException)
            {
                return Problem("Couldn't add photo");
            }
            catch
            {
                return Problem("Internal server error.");
            }
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            try
            {
                await _photoService.SetAsMain(userId, id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound($"Photo with id {id} doesn't exist");
            }
            catch (AlreadyExistsException)
            {
                return Conflict("This is a main photo already");
            }
            catch (SaveDataException)
            {
                return Problem("Error occured during saving process.");
            }
            catch
            {
                return Problem("Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            try
            {
                await _photoService.DeleteForUser(userId, id);
                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound($"Photo with id {id} doesn't exist");
            }
            catch (ForbiddenActionException)
            {
                return Conflict("You cannot delete your main photo");
            }
            catch (SaveDataException)
            {
                return Problem("Error occured during saving process.");
            }
            catch
            {
                return Problem("Internal server error.");
            }
        }

    }
}
