namespace RetCodeTester;
class Program
{
    static int Main(string[] args)
    {
        Tester tester = new Tester();
        int code = tester.Run(args);
        return code;
    }
}