namespace GatherUp.Core.Interfaces
{
    // כל מחלקה שרוצה להאזין לאירועי המערכת מממשת ממשק זה
    public interface IEventNotifier
    {
        void OnAttendanceConfirmed(int participantId, int eventId);
        void OnPaymentReceived(int participantId, int eventId);
        void OnPollAnswered(int participantId, int pollId);
        void OnPollCreated(int pollId, int eventId);
        void OnEventDetailsChanged(int eventId);
    }
}
