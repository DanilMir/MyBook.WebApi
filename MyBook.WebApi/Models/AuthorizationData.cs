﻿namespace MyBook.Models;

public class AuthorizationData
{
    public string grant_type { get; set; }
    public string username { get; set; }
    public string password { get; set; }
}