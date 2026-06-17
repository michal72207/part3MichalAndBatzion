using System;
using System.Collections.Generic;
using System.Linq;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.BL
{
    public record FinancialSummary(
        IEnumerable<(Participant Participant, decimal Amount)> Payers,
        decimal TotalIncome,
        IEnumerable<(VendorAllocation Vendor, decimal Owed)> Vendors,
        decimal TotalExpenses,
        decimal Balance
    );

    public class FinanceService
    {
        private readonly IRepository<Participant>       _participantRepo;
        private readonly IRepository<VendorAllocation>  _vendorRepo;
        private readonly IRepository<Receipt>           _receiptRepo;
        private readonly IRepository<Event>             _eventRepo;
        private readonly IEmailService                  _emailService;
        private readonly IEventNotifier?                _notifier;

        public event Action<int, int>? PaymentReceived;

        public FinanceService(
            IRepository<Participant> participantRepo,
            IRepository<VendorAllocation> vendorRepo,
            IRepository<Receipt> receiptRepo,
            IRepository<Event> eventRepo,
            IEmailService emailService,
            IEventNotifier? notifier = null)
        {
            _participantRepo = participantRepo;
            _vendorRepo      = vendorRepo;
            _receiptRepo     = receiptRepo;
            _eventRepo       = eventRepo;
            _emailService    = emailService;
            _notifier        = notifier;
        }

        public void RegisterPayment(int participantId, int eventId, decimal amount)
        {
            var participant = _participantRepo.GetById(participantId);
            participant.HasPaid = true;
            participant.AmountContributed += amount;
            _participantRepo.Update(participant);

            PaymentReceived?.Invoke(participantId, eventId);
            _notifier?.OnPaymentReceived(participantId, eventId);
        }

        public void AddVendorDebt(int vendorId, decimal amount)
        {
            var vendor = _vendorRepo.GetById(vendorId);
            vendor.AmountOwed += amount;
            _vendorRepo.Update(vendor);
        }

        // הוספת קבלה לספק - עובד על Receipt ישירות
        public void AddReceipt(Receipt receipt, int vendorId)
        {
            _receiptRepo.Add(receipt);

            var vendor = _vendorRepo.GetById(vendorId);
            vendor.ReceiptIds.Add(receipt.Id);
            _vendorRepo.Update(vendor);
        }

        public void SendPaymentReminders(int eventId, string bankDetails)
        {
            var ev = _eventRepo.GetById(eventId);

            _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id) && p.IsAttending == true && !p.HasPaid)
                .ToList()
                .ForEach(p => _emailService.Send(p.Email, "תזכורת תשלום",
                    $"שלום {p.Name},\nסכום לתשלום: {ev.PricePerParticipant} ₪\nפרטי חשבון: {bankDetails}"));
        }

        public FinancialSummary GetFinancialSummary(int eventId)
        {
            var ev       = _eventRepo.GetById(eventId);
            var payers   = CalcPayers(ev);
            var vendors  = CalcVendors(ev);
            decimal income   = payers.Sum(x => x.Item2);
            decimal expenses = vendors.Sum(x => x.Item2);
            return new FinancialSummary(payers, income, vendors, expenses, income - expenses);
        }

        // חישוב יתרה בשורה אחת
        public decimal GetNetBalance(int eventId)
        {
            var ev = _eventRepo.GetById(eventId);
            return _participantRepo.GetAll()
                       .Where(p => ev.ParticipantIds.Contains(p.Id) && p.IsAttending == true && p.HasPaid)
                       .Sum(p => p.AmountContributed)
                   - _vendorRepo.GetAll()
                       .Where(v => ev.VendorIds.Contains(v.Id))
                       .Sum(v => v.AmountOwed);
        }

        // שיטוח קבלות כל הספקים - SelectMany
        public IEnumerable<(int ReceiptId, decimal Amount, DateTime Date)> GetAllReceiptsSorted(int eventId)
        {
            var ev = _eventRepo.GetById(eventId);
            return _vendorRepo.GetAll()
                .Where(v => ev.VendorIds.Contains(v.Id))
                .SelectMany(v => v.ReceiptIds)
                .Select(id => _receiptRepo.GetById(id))
                .OrderByDescending(r => r.Date)
                .Select(r => (r.Id, r.Amount, r.Date));
        }

        private IEnumerable<(Participant, decimal)> CalcPayers(Event ev) =>
            _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id) && p.HasPaid)
                .Select(p => (p, p.AmountContributed));

        private IEnumerable<(VendorAllocation, decimal)> CalcVendors(Event ev) =>
            _vendorRepo.GetAll()
                .Where(v => ev.VendorIds.Contains(v.Id))
                .Select(v => (v, v.AmountOwed));
    }
}
