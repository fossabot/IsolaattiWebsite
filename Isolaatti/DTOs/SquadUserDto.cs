﻿using System;
using System.Collections.Generic;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;

namespace Isolaatti.DTOs;

public class SquadUserDto
{
    public UserFeed User { get; set; }
    public List<string> Permissions { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime Joined { get; set; }
    public DateTime? LastInteraction { get; set; }
    public double Ranking { get; set; }
}