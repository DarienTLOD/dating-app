﻿using AppCore.DTOs;
using AppCore.Entities;
using AppCore.HelperEntities;
using AppCore.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<(PagedList<User>, IEnumerable<UserForListDto>)> GetUsers(int id, UserParams userParams)
        {
            var userFromRepo = await _userRepo.GetUser(id);

            userParams.UserId = id;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _userRepo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            return (users, usersToReturn);
        }

        public async Task<ReturnTypes> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            var userFromRepo = await _userRepo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            return await _userRepo.SaveAll() ? ReturnTypes.Good : ReturnTypes.SaveError;
        }
    
        public async Task<UserForDetailedDTO> GetUser(int id)
        {
            var user = await _userRepo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDTO>(user);

            return userToReturn;
        }

        public async Task<ReturnTypes> LikeUser(int id, int recipientId)
        {
            var like = await _userRepo.GetLike(id, recipientId);

            if (like != null)
            {
                //return BadRequest("You have already liked this user");
                return ReturnTypes.DataError;
            }

            if (await _userRepo.GetUser(recipientId) == null)
            {
                //return NotFound();
                return ReturnTypes.NotFound;
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _userRepo.Add(like);

            return await _userRepo.SaveAll() ? ReturnTypes.Good : ReturnTypes.SaveError;
        }
    }
}