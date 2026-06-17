using System;
using System.IO;
using System.Collections.Generic;
using GatherUp.BL;
using GatherUp.Core.DO;
using GatherUp.Infrastructure.Data;

namespace GatherUp.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseDir        = AppDomain.CurrentDomain.BaseDirectory;
            string xmlFolder      = Path.Combine(baseDir, "XMLData");
            string receiptsFolder = Path.Combine(baseDir, "ReceiptsStorage");
            string emailsFolder   = Path.Combine(baseDir, "EmailsLog");

            // ── Infrastructure ──────────────────────────────────────
            var eventRepo       = new XmlRepository<Event>(xmlFolder);
            var participantRepo = new XmlRepository<Participant>(xmlFolder);
            var vendorRepo      = new XmlRepository<VendorAllocation>(xmlFolder);
            var pollRepo        = new XmlRepository<Poll>(xmlFolder);
            var receiptRepo     = new ReceiptRepository(xmlFolder, receiptsFolder);
            var emailService    = new FileEmailService(emailsFolder);

            if (!File.Exists(Path.Combine(xmlFolder, "Events.xml")))
                InitializeData.Initialize(eventRepo, participantRepo, vendorRepo, pollRepo);

            // ── הזרקת תלויות ל-BL ───────────────────────────────────
            var notifier           = new EventNotifierService(participantRepo, eventRepo, emailService);
            var participantService = new ParticipantService(participantRepo, eventRepo, emailService, notifier);
            var financeService     = new FinanceService(participantRepo, vendorRepo, receiptRepo, eventRepo, emailService, notifier);
            var pollService        = new PollService(pollRepo, eventRepo, participantRepo, emailService, notifier);

            // ── מסך כניסה: רישום משתמש חדש ─────────────────────────
            Print("מסך כניסה: רישום משתמש חדש");

            var newParticipant = new Participant
            {
                Id    = 201,
                Name  = "יוסי ישראלי",
                Email = "yosi@gmail.com",
                MailingPreferences = new List<MailingPreference>
                {
                    MailingPreference.PollCreated,
                    MailingPreference.EventChanges
                }
            };
            participantService.AddParticipantToEvent(500, newParticipant);
            Console.WriteLine($"[✓] {newParticipant.Name} נוסף לאירוע 500\n");

            // ── מסך אירוע: אישור הגעה ───────────────────────────────
            Print("מסך אירוע: לחיצה על 'אשר הגעה'");
            participantService.ConfirmAttendance(201, 500, isAttending: true);
            Console.WriteLine("[✓] אישור הגעה נרשם\n");

            // ── מסך ניהול: שליחת הזמנות המוניות ────────────────────
            Print("מסך ניהול: שליחת הזמנות המוניות");
            participantService.SendPendingInvitations(500, "https://gatherup.app/invite/500");
            Console.WriteLine("[✓] הזמנות נשלחו לכל מי שטרם השיב\n");

            // ── מסך תשלום: רישום תשלום ──────────────────────────────
            Print("מסך תשלום: לחיצה על 'אשר תשלום'");
            financeService.RegisterPayment(201, 500, amount: 150);
            Console.WriteLine("[✓] תשלום 150₪ נרשם למשתתף 201\n");

            // ── מסך ספקים: הוספת חוב ────────────────────────────────
            Print("מסך ספקים: הוספת חוב לספק");
            financeService.AddVendorDebt(vendorId: 1, amount: 500);
            Console.WriteLine("[✓] חוב 500₪ נוסף לספק 1\n");

            // ── מסך ספקים: הוספת קבלה ───────────────────────────────
            Print("מסך ספקים: הוספת קבלה");
            string dummyFile = Path.Combine(baseDir, "receipt_dummy.txt");
            File.WriteAllText(dummyFile, "קבלה מס' 1001 - קייטרינג אסאדו");
            var receipt = new Receipt { Id = 1, VendorId = 1, FilePath = dummyFile, Amount = 4500, Date = DateTime.Now };
            financeService.AddReceipt(receipt, vendorId: 1);
            Console.WriteLine("[✓] קבלה נוספה לספק 1\n");

            // ── מסך ניהול: תזכורות תשלום ────────────────────────────
            Print("מסך ניהול: שלח תזכורות תשלום");
            financeService.SendPaymentReminders(500, "בנק הפועלים 12-345-678901");
            Console.WriteLine("[✓] תזכורות נשלחו למי שאישר הגעה ולא שילם\n");

            // ── מסך תקציב: סיכום פיננסי ─────────────────────────────
            Print("מסך תקציב: סיכום פיננסי");
            var summary = financeService.GetFinancialSummary(500);
            Console.WriteLine($"הכנסות: {summary.TotalIncome} ₪ | הוצאות: {summary.TotalExpenses} ₪ | יתרה: {summary.Balance} ₪\n");

            // ── מסך אירוע: יצירת סקר חדש ────────────────────────────
            Print("מסך אירוע: יצירת סקר חדש");
            var newPoll = pollService.CreatePoll(500, "סקר מיקום סופי",
                new List<(string, List<string>)>
                {
                    ("איפה לקיים?",   new List<string> { "תל אביב", "ירושלים", "חיפה" }),
                    ("באיזה שעה?",    new List<string> { "18:00", "19:00", "20:00" })
                });
            Console.WriteLine($"[✓] סקר '{newPoll.Name}' נוצר (Id={newPoll.Id})\n");

            // ── מסך סקר: הגשת הצבעות ────────────────────────────────
            Print("מסך סקר: הגשת הצבעות");
            pollService.SubmitVote(newPoll.Id, questionId: 1, participantId: 201, answer: "ירושלים");
            pollService.SubmitVote(newPoll.Id, questionId: 1, participantId: 101, answer: "תל אביב");
            pollService.SubmitVote(newPoll.Id, questionId: 1, participantId: 201, answer: "תל אביב"); // עדכון הצבעה
            Console.WriteLine("[✓] הצבעות נרשמו (כולל עדכון הצבעה חוזרת)\n");

            // ── מסך תוצאות סקר ──────────────────────────────────────
            Print("מסך תוצאות סקר");
            var results = pollService.GetPollResults(newPoll.Id);
            foreach (var qr in results.QuestionResults)
            {
                Console.WriteLine($"  שאלה: {qr.Question.QuestionText}");
                foreach (var (option, count, pct) in qr.OptionStats)
                    Console.WriteLine($"    {option}: {count} קולות ({pct}%)");
            }

            Console.WriteLine($"\n[✓] מיילים נשמרו ב: {Path.Combine(emailsFolder, "emails.log")}");
            Console.WriteLine($"[✓] XML נשמר ב:      {xmlFolder}");
        }

        static void Print(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"=== {title} ===");
            Console.ResetColor();
        }
    }
}
