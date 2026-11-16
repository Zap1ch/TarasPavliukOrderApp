// Додаткове 3: підписник прогресу в Cli. Малює смужку щоразу, коли пройдено ще 10%.
// Не Progress<T>: у консолі він викликає обробник у фоні, і рядки можуть прийти не по порядку.
public sealed class ConsoleProgress : IProgress<int>
{
    private int _nextMark = 10;

    public void Report(int percent)
    {
        if (percent < _nextMark)
            return;

        int mark = percent / 10 * 10;   // 47 -> 40
        string bar = new string('#', mark / 10) + new string('.', 10 - mark / 10);
        Console.WriteLine($"  [{bar}] {mark,3}%");

        _nextMark = mark + 10;
    }
}