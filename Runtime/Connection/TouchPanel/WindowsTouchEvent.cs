namespace MychIO.Connection.TouchPanelDevice
{
    // Modeled off of dwFlags https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-touchinput
    public enum WindowsTouchEvent
    {

        MOVE = 1,
        DOWN = 2,
        UP = 4,
        INRANGE = 8,
        PRIMARY = 10,
        PEN = 20,
        PALM = 40,
        NOCOALESCE = 80

    }
}