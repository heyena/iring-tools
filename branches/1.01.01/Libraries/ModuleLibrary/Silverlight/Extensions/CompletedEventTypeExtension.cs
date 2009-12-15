using System;
using ModuleLibrary.Events;
using ModuleLibrary.Types;

namespace ModuleLibrary.Extensions
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
