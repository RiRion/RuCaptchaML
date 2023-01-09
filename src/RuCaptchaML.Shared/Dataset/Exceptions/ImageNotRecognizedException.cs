namespace RuCaptchaML.Shared.Dataset.Exceptions;

public class ImageNotRecognizedException : ApplicationException
{
    public ImageNotRecognizedException(){}

    public ImageNotRecognizedException(string message) : base(message){}
}
