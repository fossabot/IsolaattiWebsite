using System;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class CompleteSongRecord : Controller
    {
        private readonly DbContextApp _dbContext;

        public CompleteSongRecord(DbContextApp contextApp)
        {
            _dbContext = contextApp;
        }
        [HttpPost]
        public void Index(int songId, string bassUrl, string drumsUrl, string voiceUrl, string otherUrl)
        {
            try
            {
                Song recordToComplete = _dbContext.Songs.Find(songId);
                recordToComplete.BassUrl = bassUrl;
                recordToComplete.DrumsUrl = drumsUrl;
                recordToComplete.OtherUrl = otherUrl;
                recordToComplete.VoiceUrl = voiceUrl;
                _dbContext.Songs.Update(recordToComplete);
                _dbContext.SaveChanges();

                int userId = _dbContext.Songs.Find(recordToComplete.Id).OwnerId;
                
                // add here some code to decide if should send an email
                
                User user = _dbContext.Users.Find(userId);
                if (user.NotifyByEmail)
                {
                    var emailNotificationSender = new EmailNotification(
                        _dbContext,
                        userId,
                        EmailNotification.EmailNotificationSongReady,
                        songId
                        );
                    emailNotificationSender.Send();
                }


                if (user.NotifyWhenProcessFinishes)
                {
                    // sends a notification to the user
                    NotificationSender notificationSender = new NotificationSender(
                        NotificationSender.NotificationModeProcessesFinished,
                        recordToComplete,
                        user,
                        _dbContext
                    );
                    notificationSender.Send();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}