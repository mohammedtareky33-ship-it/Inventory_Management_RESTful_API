using InventoryBL.Interfaces;
using InventoryBL.Services;
using InventoryManagemetRESTFUL_API.Authoraization;
using InventoryShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagemetRESTFUL_API.Controllers
{


    [Authorize]
    [Route("api/Users")]
    [ApiController]
   
    public class UsersController : ControllerBase
    {
        readonly IUserService _userService ;
        public UsersController(IUserService userService) {
        
        _userService = userService;
        }
        [Authorize(Policy = "Permissions:Users")]
        [HttpGet(Name = "AllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<AllUsersDTO>> getUsers(int pageNum, int pageSize, [FromServices] IAuthorizationService authService)
        {
            //var auth=await authService.AuthorizeAsync(User,)
  
            try
            {


                var result = await _userService.getUserList(pageNum, pageSize);
                var users = new AllUsersDTO
                {
                    Users=result.users,
                    Count=result.usersCount
                };
                return Ok(users);
            }
            
            catch (Exception ex) {
                return StatusCode(500,ex.Message);
            }

            
          
        }
  
        [HttpGet("{userId}", Name = "getUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<UserReadDTO>> getUserById(int userId, [FromServices] IAuthorizationService service)
        {
            if (userId < 1)
                return BadRequest("id must be greater more 0");
            var auth = await service.AuthorizeAsync(User, userId, "PermissionUsersOrUserOwnership");
            if (!auth.Succeeded)
                return Forbid();

                var user=await _userService.getUser(userId);
                if(user == null)
                    return NotFound($"not found id ={userId}");
                return Ok(user);
            
       
            

        }
        [Authorize(Policy = "Permissions:Users")]
        [HttpGet("byUserName", Name ="byUserName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserReadDTO>> getUserByUsername(string userName)
        {
          
                var user = await _userService.getUser(userName);
                if (user == null)
                    return NotFound($"not found name ={userName}");
                return Ok(user);
        
        }
        [Authorize(Policy = "Permissions:Users")]
        [HttpPost(Name ="createUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserReadDTO>> CreateUser(UserAddDTO user)
        {
           
                user.CreatedByUserId=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                int id = await _userService.Add(user);
                var CreatedUser=new UserLightReadDTO(id,user.UserName,user.CreatedByUserId,user.Permissions);
                return CreatedAtRoute("getUserById",new {userId= id }, CreatedUser);
       

        }
        [Authorize(Policy = "Permissions:Users")]
        [HttpPatch("{userId}/updatePermissions",Name ="updatePermissions")]
        public async Task<ActionResult<UserUpdatePermissionDTO>> updatePermissions(int userId,UserUpdatePermissionDTO userUpdate)
        {
    
            userUpdate =new UserUpdatePermissionDTO(userId, userUpdate.Permission);
            var result= await _userService.UpdatePermissions(userUpdate);
 
            return Ok(userUpdate);
        }
 
        [HttpPatch("changePassword", Name = "changePassword")]
        public async Task<ActionResult> UpdatePassword( UserUpdatePasswordDTO userUpdate)
        {
       
            userUpdate =new UserUpdatePasswordDTO(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), userUpdate.Password,userUpdate.PrevPassword);
            var result =await _userService.UpdatePassword(userUpdate);

            return NoContent();
        }
        [Authorize(Policy = "Permissions:Users")]
        [HttpDelete("{userId}", Name = "DeleteUser")]
        public async Task<ActionResult> deleteUser(int userId) {
         
            await _userService.Delete(userId);
                return NoContent();
    
        }
      

        [HttpGet("Permissions", Name = "Permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
   
        public IActionResult getPermissionsMap()
        {
            return Ok(UserPermissions.PermissionsNumMap);
        } 
    }
}
