﻿using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class StartedProcessController : ControllerBase
    {
        private readonly DbContextApp db;
        public StartedProcessController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public void Index(int songId)
        {
            var song = db.Songs.Find(songId);
            song.IsBeingProcessed = true;
            db.Songs.Update(song);
            db.SaveChanges();
            User user = db.Users.Find(song.OwnerId);
            if (user.NotifyWhenProcessStarted)
            {
                NotificationSender notificationSender = new NotificationSender(
                    NotificationSender.NotificationModeSongStartedToProcess,
                    song,
                    user,
                    db
                );
                notificationSender.Send();
            }
            if (!user.NotifyByEmail) return;
            var emailNotificationSender = new EmailNotification(
                db,
                song.OwnerId,
                EmailNotification.EmailNotificationSongProcessingStarted,
                songId);
            emailNotificationSender.Send();
        }
    }
}