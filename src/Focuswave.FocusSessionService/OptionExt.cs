namespace Focuswave.FocusSessionService;

public static class OptionExt
{
    public static Option<T> Or<T>(this Option<T> first, Option<T> second) =>
        first.IsSome ? first : second;

    public static Option<T> Or<T>(this Option<T> first, Func<Option<T>> second) =>
        first.IsSome ? first : second();
}
