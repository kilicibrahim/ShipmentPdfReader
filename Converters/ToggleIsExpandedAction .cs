using ShipmentPdfReader.Services.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipmentPdfReader.Converters
{
    public class ToggleIsExpandedAction : TriggerAction<ViewCell>
    {
        protected override void Invoke(ViewCell sender)
        {
            //if (sender.BindingContext is ExtractedData data)
            //{
            //    data.IsExpanded = !data.IsExpanded;
            //}
        }
    }

}
