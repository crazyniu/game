using System;
using System.Collections.Generic;
using PerchSort.Core;

namespace PerchSort.Game
{
    // Presentation requests never commit gameplay or persistence.
    public interface IBoardView
    {
        event Action<int> BranchRequested;
        event Action SelectionCancelRequested;
        event Action<int> SwivelRequested;
        event Action UndoRequested;
        event Action<BoosterKind> BoosterRequested;
        event Action<BoosterKind> ShopRequested;
        event Action<string> GoalInfoRequested;
        event Action SettingsRequested;
        event Action DismissTopRequested;
        event Action EntranceSkipRequested;
        event Action<BoardAction> DeferredActionRequested;
        event Action HintDismissRequested;
        event Action<long> EntrancePresented;
        event Action<long> ResolutionPresented;
        event Action<string> PresentationFailed;
        void Show(BoardSnapshot snapshot);
        void UpdateHud(HudSnapshot snapshot);
        void SetSelection(SelectionPresentation selection);
        void PlayResolution(IReadOnlyList<BoardResolutionEvent> orderedEvents, long transactionId);
        void Restore(BoardSnapshot snapshot);
        void SetInput(InputPresentation input);
        void ShowHint(HintPresentation hint);
        void ShowBubble(BubblePresentation bubble);
        void SetTutorial(TutorialPresentation tutorial);
        void PlayEntrance(EntrancePresentation entrance);
        void SkipEntrance();
        void SetOverlay(OverlayPresentation overlay);
        void SetLifecycle(bool focus, BoardMotionMode motionMode);
        void HideAndCancel();
    }
}
