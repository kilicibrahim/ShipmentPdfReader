using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShipmentPdfReader
{
    public class Messages : ValueChangedMessage<string>
    {
        public Messages(string value) : base(value)
        {

        }


    }

}
