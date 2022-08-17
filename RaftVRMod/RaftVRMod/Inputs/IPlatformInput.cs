namespace RaftVR.Inputs
{
    interface IPlatformInput
    {
        void Update();
        bool IsReady();
        float GetWalk();
        float GetStrafe();
        float GetTurn();
        bool GetPrimaryAction();
        bool GetSecondaryAction();
        bool GetRepair();
        bool GetInteract();
        bool GetSprint();
        bool GetJump();
        bool GetCrouch();
        bool GetClick();
        bool GetCancel();
        bool GetBlockPick();
        bool GetRotate();
        bool GetInventory();
        bool GetNotebook();
        bool GetDrop();
        bool GetRemove();
        bool GetPause();
        bool GetNextItem();
        bool GetPreviousItem();
        bool GetPaintOneSide();
        bool GetCalibrate();
        string GetBindString(string identifier);
    }
}
