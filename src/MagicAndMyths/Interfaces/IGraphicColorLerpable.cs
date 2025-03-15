using UnityEngine;

namespace MagicAndMyths
{
    public interface IGraphicColorLerpable
    {
        Color ColorOne { get; }
        Color ColorTwo { get; }
        float ColorLerpT { get; }
    }
}
