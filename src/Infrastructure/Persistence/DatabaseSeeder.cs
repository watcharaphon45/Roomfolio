using Microsoft.EntityFrameworkCore;
using Roomfolio.Domain.Deposits;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Organizations;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(RoomfolioDbContext context)
    {
        // Apply pending migrations automatically
        await context.Database.MigrateAsync();

        if (!await context.Projects.AnyAsync())
        {
            // Projects
            var noble = new Project
            {
                Name = "Noble Ploenchit",
                Developer = "Noble Development",
                Address = "1035 Ploenchit Rd, Lumphini, Pathum Wan",
                City = "Bangkok",
                TotalFloors = 52
            };

            var ashton = new Project
            {
                Name = "Ashton Asoke",
                Developer = "Ananda Development",
                Address = "Asoke Montri Rd, Khlong Toei Nuea, Watthana",
                City = "Bangkok",
                TotalFloors = 50
            };

            var ideo = new Project
            {
                Name = "Ideo Q Sukhumvit 36",
                Developer = "Ananda Development",
                Address = "Sukhumvit 36, Khlong Tan, Khlong Toei",
                City = "Bangkok",
                TotalFloors = 47
            };

            var rhythm = new Project
            {
                Name = "Rhythm Ekkamai",
                Developer = "AP Thailand",
                Address = "Sukhumvit 63, Khlong Tan Nuea, Watthana",
                City = "Bangkok",
                TotalFloors = 32
            };

            context.Projects.AddRange(noble, ashton, ideo, rhythm);

            // Owners
            var owner1 = new Owner
            {
                FullName = "คุณสมชาย วงศ์สวัสดิ์",
                Phone = "081-445-6789",
                Email = "somchai.w@gmail.com",
                BankName = "ธ.กสิกรไทย (KBANK)",
                BankAccountNumber = "045-2-12345-6",
                LineId = "somchai_condo"
            };

            var owner2 = new Owner
            {
                FullName = "คุณภัทรินทร์ รัตนโชติ",
                Phone = "089-776-5432",
                Email = "patarin.r@outlook.com",
                BankName = "ธ.ไทยพาณิชย์ (SCB)",
                BankAccountNumber = "112-3-98765-4",
                LineId = "patarin_p"
            };

            var owner3 = new Owner
            {
                FullName = "Mrs. Sarah Jenkins",
                Phone = "+66 82 345 6789",
                Email = "sarah.j@globalasset.sg",
                BankName = "ธ.กรุงเทพ (BBL)",
                BankAccountNumber = "201-0-45678-9",
                LineId = "sarah_j_bkk"
            };

            var owner4 = new Owner
            {
                FullName = "คุณนพพร สุขเกษม",
                Phone = "086-554-1122",
                Email = "nopporn.s@gmail.com",
                BankName = "ธ.กรุงไทย (KTB)",
                BankAccountNumber = "008-1-55443-2",
                LineId = "nopporn_condo"
            };

            context.Owners.AddRange(owner1, owner2, owner3, owner4);

            // Units
            var units = new List<Unit>
            {
                new Unit
                {
                    Project = noble,
                    Owner = owner1,
                    UnitNumber = "#1402",
                    Floor = 14,
                    Building = "A",
                    Bedrooms = 1,
                    Bathrooms = 1,
                    AreaSqM = 45.5m,
                    TargetRentPrice = 38000,
                    Status = UnitStatus.Rented,
                    PaymentMode = RentalPaymentMode.DirectToOwner,
                    TenantName = "คุณกิตติศักดิ์ พ.",
                    Note = "สัญญาเช่า 1 ปี ถึง 31 ต.ค. 2026"
                },
                new Unit
                {
                    Project = ashton,
                    Owner = owner2,
                    UnitNumber = "#2208",
                    Floor = 22,
                    Building = "Main",
                    Bedrooms = 1,
                    Bathrooms = 1,
                    AreaSqM = 48.0m,
                    TargetRentPrice = 45000,
                    Status = UnitStatus.Rented,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    TenantName = "Mr. David Miller",
                    Note = "ผู้เช่าชาวต่างชาติ ค่าเช่าผ่านนายหน้า"
                },
                new Unit
                {
                    Project = ideo,
                    Owner = owner3,
                    UnitNumber = "#0915",
                    Floor = 9,
                    Building = "B",
                    Bedrooms = 1,
                    Bathrooms = 1,
                    AreaSqM = 34.0m,
                    TargetRentPrice = 22000,
                    Status = UnitStatus.Rented,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    TenantName = "คุณนพวรรณ ช.",
                    Note = "ชำระทุกวันที่ 1 ผ่านบัญชีนายหน้า"
                },
                new Unit
                {
                    Project = rhythm,
                    Owner = owner4,
                    UnitNumber = "#1104",
                    Floor = 11,
                    Building = "Main",
                    Bedrooms = 1,
                    Bathrooms = 1,
                    AreaSqM = 36.5m,
                    TargetRentPrice = 28000,
                    Status = UnitStatus.Available,
                    PaymentMode = RentalPaymentMode.DirectToOwner,
                    TenantName = null,
                    Note = "ห้องว่าง พร้อมนัดชมห้อง ทำความสะอาดแล้ว"
                },
                new Unit
                {
                    Project = noble,
                    Owner = owner1,
                    UnitNumber = "#2805",
                    Floor = 28,
                    Building = "B",
                    Bedrooms = 2,
                    Bathrooms = 2,
                    AreaSqM = 72.0m,
                    TargetRentPrice = 65000,
                    Status = UnitStatus.Available,
                    PaymentMode = RentalPaymentMode.DirectToOwner,
                    TenantName = null,
                    Note = "วิวเมือง ทิศตะวันออก ตกแต่งครบ (Fully Furnished)"
                },
                new Unit
                {
                    Project = ashton,
                    Owner = owner2,
                    UnitNumber = "#3502",
                    Floor = 35,
                    Building = "Main",
                    Bedrooms = 2,
                    Bathrooms = 2,
                    AreaSqM = 68.5m,
                    TargetRentPrice = 58000,
                    Status = UnitStatus.UnderMaintenance,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    TenantName = null,
                    Note = "กำลังซ่อมแอร์และทาสีเก็บงาน กำหนดเสร็จสัปดาห์หน้า"
                }
            };

            context.Units.AddRange(units);
            await context.SaveChangesAsync();
        }

        // Seed Tenants & Leases if not present
        if (!await context.Tenants.AnyAsync())
        {
            var unit1402 = await context.Units.FirstOrDefaultAsync(u => u.UnitNumber == "#1402");
            var unit2208 = await context.Units.FirstOrDefaultAsync(u => u.UnitNumber == "#2208");
            var unit0915 = await context.Units.FirstOrDefaultAsync(u => u.UnitNumber == "#0915");

            var tenant1 = new Tenant
            {
                FullName = "คุณกิตติศักดิ์ พูลสวัสดิ์",
                Phone = "081-998-1122",
                Email = "kittisak.p@gmail.com",
                IdCardOrPassport = "1-1002-00334-11-2",
                LineId = "kittisak_p",
                EmergencyContact = "089-112-3344 (คุณวิภาดา - ภรรยา)",
                Status = "Active",
                Note = "ผู้เช่าประวัติดี จ่ายตรงเวลาสม่ำเสมอ"
            };

            var tenant2 = new Tenant
            {
                FullName = "Mr. David Miller",
                Phone = "+66 84 123 4567",
                Email = "david.m@corporate.com",
                IdCardOrPassport = "GBR-992384102",
                LineId = "david_m_bkk",
                EmergencyContact = "+44 7911 123456 (Sarah Miller - Sister)",
                Status = "Active",
                Note = "Expat ทำงานย่านอโศก บริษัทออกสัญญาให้"
            };

            var tenant3 = new Tenant
            {
                FullName = "คุณนพวรรณ ชัยเจริญ",
                Phone = "086-554-3321",
                Email = "nopawan.c@yahoo.com",
                IdCardOrPassport = "3-1005-00128-44-1",
                LineId = "nopawan_c",
                EmergencyContact = "081-443-2211 (คุณแม่)",
                Status = "Active",
                Note = "อาจารย์พิเศษจุฬาฯ"
            };

            context.Tenants.AddRange(tenant1, tenant2, tenant3);
            await context.SaveChangesAsync();

            var leases = new List<Lease>();

            if (unit1402 != null)
            {
                leases.Add(new Lease
                {
                    UnitId = unit1402.Id,
                    TenantId = tenant1.Id,
                    LeaseNumber = "LS-2025-081",
                    StartDate = new DateOnly(2025, 11, 1),
                    EndDate = new DateOnly(2026, 10, 31),
                    RentDueDay = 1,
                    MonthlyRent = 38000,
                    PaymentMode = RentalPaymentMode.DirectToOwner,
                    DepositAmount = 76000,
                    DepositHolder = DepositHolder.Owner,
                    Status = LeaseStatus.ExpiringSoon,
                    SignedAt = new DateOnly(2025, 10, 25),
                    Note = "สัญญา 1 ปี จ่ายตรงเจ้าของห้อง"
                });
            }

            if (unit2208 != null)
            {
                leases.Add(new Lease
                {
                    UnitId = unit2208.Id,
                    TenantId = tenant2.Id,
                    LeaseNumber = "LS-2026-012",
                    StartDate = new DateOnly(2026, 2, 15),
                    EndDate = new DateOnly(2027, 2, 14),
                    RentDueDay = 5,
                    MonthlyRent = 45000,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    DepositAmount = 90000,
                    DepositHolder = DepositHolder.Agency,
                    Status = LeaseStatus.Active,
                    SignedAt = new DateOnly(2026, 2, 10),
                    Note = "สัญญา 1 ปี รับเงินผ่านนายหน้า โอนส่งเจ้าของทุกวันที่ 10"
                });
            }

            if (unit0915 != null)
            {
                leases.Add(new Lease
                {
                    UnitId = unit0915.Id,
                    TenantId = tenant3.Id,
                    LeaseNumber = "LS-2026-034",
                    StartDate = new DateOnly(2026, 6, 1),
                    EndDate = new DateOnly(2027, 5, 31),
                    RentDueDay = 1,
                    MonthlyRent = 22000,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    DepositAmount = 44000,
                    DepositHolder = DepositHolder.Agency,
                    Status = LeaseStatus.Active,
                    SignedAt = new DateOnly(2026, 5, 20),
                    Note = "สัญญา 1 ปี ผ่านนายหน้า"
                });
            }

            if (leases.Count > 0)
            {
                context.Leases.AddRange(leases);
                await context.SaveChangesAsync();
            }
        }

        // Seed RentCharges & Payments for October 2026
        if (!await context.RentCharges.AnyAsync())
        {
            var leases = await context.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                .ToListAsync();

            var lease1402 = leases.FirstOrDefault(l => l.LeaseNumber == "LS-2025-081");
            var lease2208 = leases.FirstOrDefault(l => l.LeaseNumber == "LS-2026-012");
            var lease0915 = leases.FirstOrDefault(l => l.LeaseNumber == "LS-2026-034");

            var octStart = new DateOnly(2026, 10, 1);
            var octEnd = new DateOnly(2026, 10, 31);

            // 1. Noble Ploenchit #1402 (Direct to Owner - Fully Paid)
            if (lease1402 != null)
            {
                var charge1 = new RentCharge
                {
                    LeaseId = lease1402.Id,
                    ChargeType = "Rent",
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    DueDate = new DateOnly(2026, 10, 1),
                    Description = "ค่าเช่าห้อง #1402 ประจำเดือน ต.ค. 2026",
                    Amount = 38000,
                    Status = RentChargeStatus.Paid
                };
                context.RentCharges.Add(charge1);
                await context.SaveChangesAsync();

                var payment1 = new Payment
                {
                    PayerPartyType = PaymentParty.Tenant,
                    PayerTenantId = lease1402.TenantId,
                    ReceivedByParty = PaymentParty.Owner,
                    PaymentMode = RentalPaymentMode.DirectToOwner,
                    PaymentDate = new DateOnly(2026, 10, 1),
                    Amount = 38000,
                    Method = PaymentMethod.BankTransfer,
                    ReferenceNo = "KBANK-20261001-88412",
                    Status = PaymentStatus.Confirmed,
                    Note = "ผู้เช่าแจ้งโอนตรงเข้าบัญชีเจ้าของห้องแล้ว เจ้าของยืนยันรับเงิน"
                };
                context.Payments.Add(payment1);
                await context.SaveChangesAsync();

                var alloc1 = new PaymentAllocation
                {
                    PaymentId = payment1.Id,
                    RentChargeId = charge1.Id,
                    AllocatedAmount = 38000,
                    Note = "ชำระเต็มจำนวน"
                };
                context.PaymentAllocations.Add(alloc1);
            }

            // 2. Ashton Asoke #2208 (Through Agent - Fully Paid, cash held by agency)
            if (lease2208 != null)
            {
                var charge2 = new RentCharge
                {
                    LeaseId = lease2208.Id,
                    ChargeType = "Rent",
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    DueDate = new DateOnly(2026, 10, 5),
                    Description = "ค่าเช่าห้อง #2208 ประจำเดือน ต.ค. 2026",
                    Amount = 45000,
                    Status = RentChargeStatus.Paid
                };
                context.RentCharges.Add(charge2);
                await context.SaveChangesAsync();

                var payment2 = new Payment
                {
                    PayerPartyType = PaymentParty.Tenant,
                    PayerTenantId = lease2208.TenantId,
                    ReceivedByParty = PaymentParty.Agency,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    PaymentDate = new DateOnly(2026, 10, 4),
                    Amount = 45000,
                    Method = PaymentMethod.BankTransfer,
                    ReferenceNo = "SCB-20261004-99214",
                    Status = PaymentStatus.Confirmed,
                    Note = "โอนเข้าบัญชีนายหน้าเรียบร้อย รอส่งมอบเจ้าของในรอบ settlement"
                };
                context.Payments.Add(payment2);
                await context.SaveChangesAsync();

                var alloc2 = new PaymentAllocation
                {
                    PaymentId = payment2.Id,
                    RentChargeId = charge2.Id,
                    AllocatedAmount = 45000,
                    Note = "ชำระเต็มจำนวน เงินอยู่นายหน้า"
                };
                context.PaymentAllocations.Add(alloc2);
            }

            // 3. Ideo Q Sukhumvit 36 #0915 (Through Agent - Partially Paid 12,000 / 22,000)
            if (lease0915 != null)
            {
                var charge3 = new RentCharge
                {
                    LeaseId = lease0915.Id,
                    ChargeType = "Rent",
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    DueDate = new DateOnly(2026, 10, 1),
                    Description = "ค่าเช่าห้อง #0915 ประจำเดือน ต.ค. 2026",
                    Amount = 22000,
                    Status = RentChargeStatus.PartiallyPaid
                };
                context.RentCharges.Add(charge3);
                await context.SaveChangesAsync();

                var payment3 = new Payment
                {
                    PayerPartyType = PaymentParty.Tenant,
                    PayerTenantId = lease0915.TenantId,
                    ReceivedByParty = PaymentParty.Agency,
                    PaymentMode = RentalPaymentMode.ThroughAgent,
                    PaymentDate = new DateOnly(2026, 10, 1),
                    Amount = 12000,
                    Method = PaymentMethod.PromptPayQR,
                    ReferenceNo = "PROMPT-20261001-3341",
                    Status = PaymentStatus.Confirmed,
                    Note = "จ่ายก้อนแรก 12,000 บาท ส่วนที่เหลือ 10,000 บาท ขอนัดชำระวันที่ 25 ต.ค."
                };
                context.Payments.Add(payment3);
                await context.SaveChangesAsync();

                var alloc3 = new PaymentAllocation
                {
                    PaymentId = payment3.Id,
                    RentChargeId = charge3.Id,
                    AllocatedAmount = 12000,
                    Note = "จัดสรรงวด ต.ค. (คงค้าง 10,000)"
                };
                context.PaymentAllocations.Add(alloc3);
            }

            await context.SaveChangesAsync();
        }

        // Seed Owner Settlements for October 2026
        if (!await context.OwnerSettlements.AnyAsync())
        {
            var owners = await context.Owners.Include(o => o.Units).ToListAsync();
            var ownerPatarin = owners.FirstOrDefault(o => o.FullName.Contains("ภัทรินทร์"));
            var ownerSarah = owners.FirstOrDefault(o => o.FullName.Contains("Sarah"));
            var ownerSomchai = owners.FirstOrDefault(o => o.FullName.Contains("สมชาย"));

            var octStart = new DateOnly(2026, 10, 1);
            var octEnd = new DateOnly(2026, 10, 31);

            // 1. Owner Patarin (Ashton Asoke #2208, Through Agent: Gross 45,000, 5% fee 2,250, Net 42,750, Status: Approved)
            if (ownerPatarin != null)
            {
                var s1 = new OwnerSettlement
                {
                    OwnerId = ownerPatarin.Id,
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    Status = SettlementStatus.Approved,
                    GrossRentCollected = 45000,
                    ExpensesDeducted = 0,
                    ManagementFeesDeducted = 2250,
                    CommissionsDeducted = 0,
                    OtherAdjustments = 0,
                    NetPayable = 42750,
                    ApprovedAt = DateTimeOffset.UtcNow,
                    Note = "ตรวจทานยอดค่าเช่าและหักค่าบริหาร 5% อนุมัติเตรียมโอนเข้าบัญชี SCB"
                };
                context.OwnerSettlements.Add(s1);
                await context.SaveChangesAsync();

                var unit2208 = ownerPatarin.Units.FirstOrDefault(u => u.UnitNumber == "#2208");
                context.OwnerSettlementLines.AddRange(
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s1.Id,
                        LineType = SettlementLineType.RentCollected,
                        UnitId = unit2208?.Id,
                        Description = "ค่าเช่าห้อง #2208 งวด ต.ค. 2026 (Mr. David Miller)",
                        Amount = 45000
                    },
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s1.Id,
                        LineType = SettlementLineType.ManagementFee,
                        UnitId = unit2208?.Id,
                        Description = "ค่าบริการบริหารห้องเช่ารายเดือน (5%)",
                        Amount = -2250
                    }
                );
            }

            // 2. Owner Sarah Jenkins (Ideo Q Sukhumvit 36 #0915, Gross 22,000, Fee 1,100, Net 20,900, Status: Paid)
            if (ownerSarah != null)
            {
                var s2 = new OwnerSettlement
                {
                    OwnerId = ownerSarah.Id,
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    Status = SettlementStatus.Paid,
                    GrossRentCollected = 22000,
                    ExpensesDeducted = 0,
                    ManagementFeesDeducted = 1100,
                    CommissionsDeducted = 0,
                    OtherAdjustments = 0,
                    NetPayable = 20900,
                    ApprovedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    PaidAt = DateTimeOffset.UtcNow.AddDays(-1),
                    TransferReference = "BBL-SETTLE-20261010-091",
                    Note = "โอนเข้าบัญชี BBL เรียบร้อยแล้ว พร้อมส่งใบสรุปยอดทางอีเมล"
                };
                context.OwnerSettlements.Add(s2);
                await context.SaveChangesAsync();

                var unit0915 = ownerSarah.Units.FirstOrDefault(u => u.UnitNumber == "#0915");
                context.OwnerSettlementLines.AddRange(
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s2.Id,
                        LineType = SettlementLineType.RentCollected,
                        UnitId = unit0915?.Id,
                        Description = "ค่าเช่าห้อง #0915 งวด ต.ค. 2026 (คุณนพวรรณ ช.)",
                        Amount = 22000
                    },
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s2.Id,
                        LineType = SettlementLineType.ManagementFee,
                        UnitId = unit0915?.Id,
                        Description = "ค่าบริหารจัดการห้องชุดรายเดือน (5%)",
                        Amount = -1100
                    }
                );
            }

            // 3. Owner Somchai (Noble Ploenchit, Gross 95,000, Expense 1,500, Fee 4,750, Net 88,750, Status: Review)
            if (ownerSomchai != null)
            {
                var s3 = new OwnerSettlement
                {
                    OwnerId = ownerSomchai.Id,
                    PeriodStart = octStart,
                    PeriodEnd = octEnd,
                    Status = SettlementStatus.Review,
                    GrossRentCollected = 95000,
                    ExpensesDeducted = 1500,
                    ManagementFeesDeducted = 4750,
                    CommissionsDeducted = 0,
                    OtherAdjustments = 0,
                    NetPayable = 88750,
                    Note = "อยู่ระหว่างตรวจทานใบเสร็จค่าแม่บ้านทำความสะอาด 1,500 บาท"
                };
                context.OwnerSettlements.Add(s3);
                await context.SaveChangesAsync();

                var unit1402 = ownerSomchai.Units.FirstOrDefault(u => u.UnitNumber == "#1402");
                context.OwnerSettlementLines.AddRange(
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s3.Id,
                        LineType = SettlementLineType.RentCollected,
                        UnitId = unit1402?.Id,
                        Description = "ค่าเช่าห้อง Noble Ploenchit งวด ต.ค. 2026",
                        Amount = 95000
                    },
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s3.Id,
                        LineType = SettlementLineType.Expense,
                        UnitId = unit1402?.Id,
                        Description = "ค่าบริการทำความสะอาดห้องชุด Big Cleaning",
                        Amount = -1500
                    },
                    new OwnerSettlementLine
                    {
                        OwnerSettlementId = s3.Id,
                        LineType = SettlementLineType.ManagementFee,
                        UnitId = unit1402?.Id,
                        Description = "ค่าธรรมเนียมบริหารปล่อยเช่า (5%)",
                        Amount = -4750
                    }
                );
            }

            await context.SaveChangesAsync();
        }

        // Seed initial expenses if none exist
        if (!await context.Expenses.AnyAsync())
        {
            var u1 = await context.Units.Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#1402");
            var u2 = await context.Units.Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#2208");
            var u3 = await context.Units.Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#0915");

            if (u1 != null && u2 != null && u3 != null)
            {
                context.Expenses.AddRange(
                    new Expense
                    {
                        UnitId = u1.Id,
                        Category = ExpenseCategory.Cleaning,
                        Description = "Deep cleaning ก่อนผู้เช่าเข้าอยู่",
                        Amount = 1500,
                        PaidBy = ExpensePaidBy.Agency,
                        IsDeductibleFromOwner = true,
                        Status = ExpenseStatus.Confirmed,
                        IncurredDate = DateTimeOffset.UtcNow.AddDays(-10),
                        Note = "แนบใบเสร็จแม่บ้านทำความสะอาดเรียบร้อย"
                    },
                    new Expense
                    {
                        UnitId = u2.Id,
                        Category = ExpenseCategory.Keycard,
                        Description = "ทำบัตรคอนโดเพิ่ม 1 ใบ",
                        Amount = 800,
                        PaidBy = ExpensePaidBy.Agency,
                        IsDeductibleFromOwner = true,
                        Status = ExpenseStatus.Confirmed,
                        IncurredDate = DateTimeOffset.UtcNow.AddDays(-14),
                        Note = "ออกใบเสร็จจากนิติบุคคลอาคารชุด"
                    },
                    new Expense
                    {
                        UnitId = u3.Id,
                        Category = ExpenseCategory.CommonFee,
                        Description = "ค่าส่วนกลางประจำปี เจ้าของชำระตรงกับนิติ",
                        Amount = 18500,
                        PaidBy = ExpensePaidBy.Owner,
                        IsDeductibleFromOwner = false,
                        Status = ExpenseStatus.Confirmed,
                        IncurredDate = DateTimeOffset.UtcNow.AddDays(-20),
                        Note = "บันทึกเพื่อรายงานสถิติค่าใช้จ่ายประจำห้อง"
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        // Seed initial security deposits if none exist
        if (!await context.SecurityDeposits.AnyAsync())
        {
            var leases = await context.Leases
                .Include(l => l.Unit)
                .Include(l => l.Tenant)
                .ToListAsync();

            foreach (var lease in leases)
            {
                var deposit = new SecurityDeposit
                {
                    LeaseId = lease.Id,
                    Holder = lease.DepositHolder,
                    InitialAmount = lease.DepositAmount > 0 ? lease.DepositAmount : lease.MonthlyRent * 2,
                    DeductedAmount = 0,
                    RefundedAmount = 0,
                    Status = DepositStatus.Held,
                    ReceivedDate = lease.StartDate,
                    BankAccountDetails = lease.DepositHolder == DepositHolder.Agency 
                        ? "KBANK - บัญชีเงินประกันบริษัท Roomfolio" 
                        : "โอนตรงเข้าบัญชีเจ้าของห้อง",
                    Note = "เงินประกันการเช่าตามสัญญา 2 เดือน"
                };

                if (lease.Unit != null && lease.Unit.UnitNumber.Contains("0915"))
                {
                    deposit.DeductedAmount = 2500;
                    deposit.Status = DepositStatus.PartiallyDeducted;
                    deposit.Transactions.Add(new DepositTransaction
                    {
                        Type = DepositTransactionType.Deduction,
                        Amount = 2500,
                        TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                        Description = "หักชดเชยค่าทำความสะอาดม่านและพรมห้องรับแขก",
                        ReferenceNo = "DED-2026-001",
                        Note = "ช่างทำความสะอาดพิเศษก่อนเปิดแอร์"
                    });
                }

                context.SecurityDeposits.Add(deposit);
            }

            await context.SaveChangesAsync();
        }

        // Seed initial agency fees if none exist
        if (!await context.AgencyFees.AnyAsync())
        {
            var u1 = await context.Units.Include(u => u.Owner).Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#1402");
            var u2 = await context.Units.Include(u => u.Owner).Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#2208");
            var u3 = await context.Units.Include(u => u.Owner).Include(u => u.Project).FirstOrDefaultAsync(u => u.UnitNumber == "#0915");

            var l1 = await context.Leases.FirstOrDefaultAsync(l => l.UnitId == (u1 != null ? u1.Id : Guid.Empty));
            var l2 = await context.Leases.FirstOrDefaultAsync(l => l.UnitId == (u2 != null ? u2.Id : Guid.Empty));
            var l3 = await context.Leases.FirstOrDefaultAsync(l => l.UnitId == (u3 != null ? u3.Id : Guid.Empty));

            if (u1 != null && u2 != null && u3 != null)
            {
                context.AgencyFees.AddRange(
                    new AgencyFee
                    {
                        UnitId = u1.Id,
                        OwnerId = u1.OwnerId,
                        LeaseId = l1?.Id,
                        FeeType = AgencyFeeType.MonthlyManagement,
                        Description = "ค่าบริการบริหารจัดการห้องเช่ารายเดือน",
                        CalculationBasis = "5% ของค่าเช่า ฿38,000",
                        Period = "ต.ค. 2026",
                        Amount = 1900,
                        CollectionMethod = FeeCollectionMethod.DeductFromSettlement,
                        Status = AgencyFeeStatus.PendingDeduction,
                        Note = "หักรอบโอนเงินค่าเช่าให้เจ้าของห้อง"
                    },
                    new AgencyFee
                    {
                        UnitId = u2.Id,
                        OwnerId = u2.OwnerId,
                        LeaseId = l2?.Id,
                        FeeType = AgencyFeeType.LeasingCommission,
                        Description = "ค่านายหน้าจัดหาผู้เช่าสัญญาใหม่ 1 ปี",
                        CalculationBasis = "ค่าเช่า 1 เดือน (฿45,000)",
                        Period = "สัญญาใหม่ 1 ปี (15 ก.พ. 2569 - 14 ก.พ. 2570)",
                        Amount = 45000,
                        CollectionMethod = FeeCollectionMethod.DeductFromSettlement,
                        Status = AgencyFeeStatus.Collected,
                        CollectedAt = DateTimeOffset.UtcNow.AddDays(-20),
                        Note = "หักในรอบโอนงวดแรกสำเร็จแล้ว"
                    },
                    new AgencyFee
                    {
                        UnitId = u3.Id,
                        OwnerId = u3.OwnerId,
                        LeaseId = l3?.Id,
                        FeeType = AgencyFeeType.MonthlyManagement,
                        Description = "ค่าบริการบริหารจัดหาและดูแลรายเดือน (Fixed Rate)",
                        CalculationBasis = "Fixed ฿2,000 / เดือน",
                        Period = "ต.ค. 2026",
                        Amount = 2000,
                        CollectionMethod = FeeCollectionMethod.SeparateInvoice,
                        Status = AgencyFeeStatus.Invoiced,
                        InvoiceNo = "INV-2026-0042",
                        Note = "ส่งใบแจ้งหนี้ไปยัง Mrs. Sarah Jenkins เรียบร้อย"
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        if (!await context.Organizations.AnyAsync())
        {
            context.Organizations.Add(new Organization
            {
                Name = "Prime Living Agency Co., Ltd.",
                TaxId = "0105565012345",
                DefaultCurrency = "THB (฿)",
                Timezone = "Asia/Bangkok (UTC+07:00)",
                PlanName = "Pro Plan",
                PlanPrice = 799,
                RoomLimit = 200,
                Address = "88/1 Sukhumvit 55, Thonglor, Watthana, Bangkok 10110",
                Phone = "02-712-9988",
                Email = "contact@primeliving.co.th"
            });
            await context.SaveChangesAsync();
        }
    }
}



