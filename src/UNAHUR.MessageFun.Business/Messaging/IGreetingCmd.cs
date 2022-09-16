namespace UNAHUR.MessageFun.Business.Messaging
{
    public interface IGreetingCmd
    {
        string Name { get; set; }
    }

    public class GreetingCmdResponse
    {
        public string Saludo { get; set; }
    }
}
