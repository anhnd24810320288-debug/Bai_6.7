/* =====================================================================
   BÀI 6.7 - CLINIC & PHARMACY MANAGEMENT SYSTEM
   Script khởi tạo CSDL ClinicDB (SQL Server / LocalDB) + dữ liệu mẫu
   Chạy toàn bộ script trong SSMS (Server: (localdb)\MSSQLLocalDB)
   ===================================================================== */
USE master;
GO
IF DB_ID(N'ClinicDB') IS NOT NULL
BEGIN
    ALTER DATABASE ClinicDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ClinicDB;
END
GO
CREATE DATABASE ClinicDB;
GO
USE ClinicDB;
GO

/* ---------- 1. BenhNhan ---------- */
CREATE TABLE BenhNhan (
    MaBN         INT IDENTITY(1,1) NOT NULL,
    HoTen        NVARCHAR(100)     NOT NULL,
    SoDienThoai  VARCHAR(15)       NULL,
    NamSinh      INT               NULL,
    CONSTRAINT PK_BenhNhan PRIMARY KEY (MaBN),
    CONSTRAINT UQ_BenhNhan_SDT UNIQUE (SoDienThoai)
);
GO

/* ---------- 2. HoSoKham ---------- */
CREATE TABLE HoSoKham (
    MaHoSo      INT IDENTITY(1,1) NOT NULL,
    MaBN        INT               NOT NULL,
    NgayKham    DATETIME          NOT NULL CONSTRAINT DF_HoSoKham_NgayKham DEFAULT GETDATE(),
    TrieuChung  NVARCHAR(MAX)     NULL,
    ChanDoan    NVARCHAR(250)     NULL,
    CONSTRAINT PK_HoSoKham PRIMARY KEY (MaHoSo),
    CONSTRAINT FK_HoSoKham_BenhNhan FOREIGN KEY (MaBN) REFERENCES BenhNhan(MaBN)
);
GO

/* ---------- 3. Thuoc ---------- */
CREATE TABLE Thuoc (
    MaThuoc     INT IDENTITY(1,1) NOT NULL,
    TenThuoc    NVARCHAR(150)     NOT NULL,
    DonViTinh   NVARCHAR(20)      NULL,
    DonGia      DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Thuoc_DonGia DEFAULT 0,
    SoLuongTon  INT               NOT NULL CONSTRAINT DF_Thuoc_SoLuongTon DEFAULT 0,
    CONSTRAINT PK_Thuoc PRIMARY KEY (MaThuoc),
    CONSTRAINT CK_Thuoc_SoLuongTon CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_Thuoc_DonGia CHECK (DonGia >= 0)
);
GO

/* ---------- 4. DonThuoc ---------- */
CREATE TABLE DonThuoc (
    MaDon      INT IDENTITY(1,1) NOT NULL,
    MaHoSo     INT               NOT NULL,
    TienKham   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonThuoc_TienKham DEFAULT 0,
    TongTien   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonThuoc_TongTien DEFAULT 0,
    TrangThai  NVARCHAR(30)      NOT NULL CONSTRAINT DF_DonThuoc_TrangThai DEFAULT N'Chờ lấy thuốc',
    CONSTRAINT PK_DonThuoc PRIMARY KEY (MaDon),
    CONSTRAINT FK_DonThuoc_HoSoKham FOREIGN KEY (MaHoSo) REFERENCES HoSoKham(MaHoSo),
    CONSTRAINT CK_DonThuoc_TrangThai CHECK (TrangThai IN (N'Chờ lấy thuốc', N'Đã thanh toán'))
);
GO

/* ---------- 5. ChiTietDonThuoc ---------- */
CREATE TABLE ChiTietDonThuoc (
    MaDon     INT           NOT NULL,
    MaThuoc   INT           NOT NULL,
    SoLuong   INT           NOT NULL,
    LieuDung  NVARCHAR(100) NULL,
    CONSTRAINT PK_ChiTietDonThuoc PRIMARY KEY (MaDon, MaThuoc),
    CONSTRAINT FK_CTDT_DonThuoc FOREIGN KEY (MaDon)   REFERENCES DonThuoc(MaDon),
    CONSTRAINT FK_CTDT_Thuoc    FOREIGN KEY (MaThuoc) REFERENCES Thuoc(MaThuoc),
    CONSTRAINT CK_CTDT_SoLuong CHECK (SoLuong > 0)
);
GO

/* ---------- DỮ LIỆU MẪU ---------- */
INSERT INTO BenhNhan (HoTen, SoDienThoai, NamSinh) VALUES
(N'Nguyễn Văn An',  '0901234567', 1990),
(N'Trần Thị Bích',  '0912345678', 1985),
(N'Lê Quang Cường', '0923456789', 2001),
(N'Phạm Thu Dung',  '0934567890', 1978);
GO

-- Tồn kho đã được trừ sẵn cho các đơn mẫu bên dưới
INSERT INTO Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon) VALUES
(N'Paracetamol 500mg',    N'Viên', 1500,   490),
(N'Amoxicillin 500mg',    N'Viên', 3000,   286),
(N'Vitamin C 1000mg',     N'Vỉ',   25000,  80),
(N'Siro ho Prospan',      N'Chai', 85000,  39),
(N'Omeprazol 20mg',       N'Viên', 2500,   186),
(N'Loratadin 10mg',       N'Viên', 2000,   250),
(N'Oresol',               N'Gói',  5000,   115),
(N'Cồn sát khuẩn 70 độ',  N'Chai', 15000,  3);   -- tồn ít để test cảnh báo thiếu thuốc
GO

INSERT INTO HoSoKham (MaBN, NgayKham, TrieuChung, ChanDoan) VALUES
(1, DATEADD(DAY, -3, GETDATE()), N'Đau họng, ho, sốt nhẹ 2 ngày',        N'Viêm họng cấp'),
(2, DATEADD(DAY, -1, GETDATE()), N'Đau bụng, đầy hơi, tiêu chảy',        N'Rối loạn tiêu hóa'),
(3, GETDATE(),                   N'Hắt hơi, ngứa mũi, chảy nước mũi',    NULL),
(4, GETDATE(),                   N'Mệt mỏi, chóng mặt, ăn uống kém',     NULL);
GO

INSERT INTO DonThuoc (MaHoSo, TienKham, TongTien, TrangThai) VALUES
(1, 100000, 242000, N'Đã thanh toán'),   -- 100.000 + 10*1.500 + 14*3.000 + 1*85.000
(2, 100000, 160000, N'Chờ lấy thuốc');   -- 100.000 + 14*2.500 + 5*5.000
GO

INSERT INTO ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) VALUES
(1, 1, 10, N'Sáng 1 viên, Chiều 1 viên sau ăn'),
(1, 2, 14, N'Sáng 1 viên, Tối 1 viên sau ăn'),
(1, 4, 1,  N'Ngày 3 lần, mỗi lần 5ml'),
(2, 5, 14, N'Sáng 1 viên trước ăn'),
(2, 7, 5,  N'Pha 1 gói với 200ml nước, uống khi tiêu chảy');
GO

SELECT * FROM BenhNhan;
SELECT * FROM HoSoKham;
SELECT * FROM Thuoc;
SELECT * FROM DonThuoc;
SELECT * FROM ChiTietDonThuoc;
GO
