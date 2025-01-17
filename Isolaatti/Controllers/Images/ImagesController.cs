﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Enums;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Isolaatti.Controllers.Images;

[ApiController]
[Route("/api/images")]
public class ImagesController : IsolaattiController
{
    private readonly IAccounts _accounts;
    private readonly ImagesService _images;
    private readonly SquadsRepository _squadsRepository;
    private readonly DbContextApp _db;

    public ImagesController(IAccounts accounts, ImagesService images, SquadsRepository squadsRepository, DbContextApp db)
    {
        _accounts = accounts;
        _images = images;
        _squadsRepository = squadsRepository;
        _db = db;
    }

    [HttpGet]
    [Route("image/{imageId}")]
    public async Task<IActionResult> GetImage(string imageId, [FromQuery] string mode)
    {
        var url = await _images.GetImageDownloadUrl(imageId, mode);
        Response.Headers.CacheControl = new StringValues(new []{"max-age=604800", $"etag={imageId}"});
        return url != null ? Redirect(url) : NotFound();
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateImage([FromForm] IFormFile file, [FromForm] string name, [FromQuery] bool setAsProfile, [FromForm] Guid? squadId)
    {
        if (squadId != null)
        {
            var squad = await _squadsRepository.GetSquad(squadId.Value);
            if (squad == null)
            {
                return BadRequest(new { error = "Squad doesn't exist, image not created" });
            }

            if (!await _squadsRepository.UserBelongsToSquad(User.Id, squad.Id))
            {
                return Unauthorized(new { error = "User cannot set image to squad" });
            }
        }

        var image = await _images.CreateImage(file.OpenReadStream(), User.Id, name, squadId);
        if (!setAsProfile) return Ok(image);
        if (squadId != null)
        {
            await _squadsRepository.SetSquadImage(squadId.Value, image.Id);
        }
        else
        {
            User.ProfileImageId = image.Id;
            _db.Users.Update(User);
            await _db.SaveChangesAsync();
        }
        return Ok(image);
    }

    [HttpGet]
    [Route("profile_image/of_user/{userId:int}")]
    public async Task<IActionResult> GetProfileImageOfUserById(int userId, string mode = "original")
    {
        var user = await _db.Users.FindAsync(userId);
        if (user?.ProfileImageId == null)
        {
            return Redirect(user?.ProfileImageUrl ?? "/res/imgs/avatar.svg");
        }
        
        var url = await _images.GetImageDownloadUrl(user?.ProfileImageId, mode);

        return Redirect(url);
    }

    [HttpGet]
    [Route("of_user/{userId:int}")]
    public async Task<IActionResult> GetImagesOfUserById(int userId, string? lastId)
    {
        return Ok(await _images.GetImagesOfUser(userId, lastId));
    }

    [HttpGet]
    [Route("of_squad/{squadId:guid}")]
    public async Task<IActionResult> GetImagesOfSquadById(Guid squadId, string lastId)
    {
        return Ok(await _squadsRepository.GetImagesOfSquad(squadId, lastId));
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("set_image_of_squad/{squadId:guid}")]
    public async Task<IActionResult> SetSquadImage(Guid squadId, [FromQuery] string imageId)
    {
        var image = await _images.GetImage(imageId);
        if (image == null)
        {
            return BadRequest("Image not found");
        }
        
        await _squadsRepository.SetSquadImage(squadId, imageId);

        return Ok();
    }

    [IsolaattiAuth]
    [HttpDelete]
    [Route("{imageId}")]
    public async Task<IActionResult> DeleteImages(string imageId)
    {
        var result = await _images.DeleteImage(imageId, User.Id);

        return result switch
        {
            ImageModificationResult.Success => Ok(new { message = "Success" }),
            ImageModificationResult.Error => Problem("Internal error"),
            ImageModificationResult.NotFound => NotFound(),
            ImageModificationResult.NotOwned => Unauthorized(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{imageId}/rename")]
    public async Task<IActionResult> RenameImage(string imageId, SimpleStringData payload)
    {
        var result = await _images.RenameImage(imageId, User.Id, payload.Data);

        return result switch
        {
            ImageModificationResult.Success => Ok(new { message = "Success" }),
            ImageModificationResult.Error => Problem("Internal error"),
            ImageModificationResult.NotFound => NotFound(),
            ImageModificationResult.NotOwned => Unauthorized(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

}
