namespace NS.UnifiedLoot.UnifiedLoot.Runtime.Core {
    /// <summary>
    /// Implemented by stateful strategies that support full state reset.
    /// Use <c>pipeline.Strategies.OfType&lt;IResettable&gt;()</c> to reset all stateful
    /// strategies from Unity lifecycle hooks (OnDestroy, scene reload, etc.)
    /// without importing concrete strategy types.
    /// </summary>
    public interface IResettable {
        /// <summary>
        /// Resets all internal state tracked by this strategy.
        /// </summary>
        void ResetAll();
    }
}
