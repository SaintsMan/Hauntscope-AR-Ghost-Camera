using System;

namespace Hauntscope.Core.Services
{
    public interface IBackButton
    {
        event Action Pressed;
    }
}
