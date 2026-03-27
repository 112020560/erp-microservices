using System;
using AccessControl.Domain.Entities;

namespace AccessControl.Application.Abstractions;

public interface ITokenProvider
{
    string Create(User user);
}
