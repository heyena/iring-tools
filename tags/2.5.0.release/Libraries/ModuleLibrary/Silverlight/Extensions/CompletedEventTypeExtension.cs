using System;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;

namespace org.iringtools.modulelibrary.extensions
{
  public static class CompletedEventTypeExtension
  {
    public static bool CheckForType(this CompletedEventArgs args, CompletedEventType enumToCheckFor)
    {
      CompletedEventType EventType = (CompletedEventType)
        Enum.Parse(typeof(CompletedEventType), args.CompletedType.ToString(), true);

      return EventType == enumToCheckFor;
    }

  }
}
