﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Isolaatti.Models
{
    public class FollowerRelation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
    }
}