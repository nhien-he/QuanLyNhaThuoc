using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
	public partial class Form1 : Form
	{
		private Panel sidebarPanel, headerPanel, mainContentPanel;
		private Label lblHeaderTitle;

		// Data POS
		private List<ChiTietGioHang> gioHang = new List<ChiTietGioHang>();
		private KhachHang selectedKhachHangPos = null;

		// Biến cờ (flag) tránh vòng lặp load lại grid khi chọn dòng SĐT
		private bool isUpdatingSearch = false;

		// Controls POS Left
		private DataGridView dgvPosThuoc, dgvPosGioHang;
		private TextBox txtPosSearchThuoc;
		private NumericUpDown numPosSoLuong;
		private Label lblPosTongTien, lblPosDiemQuyDoi;

		// Controls POS Right (Thẻ VIP & Quy đổi điểm)
		private DataGridView dgvKhachHangPos;
		private TextBox txtPosSearchKhach;
		private Label lblPosKhachTen, lblPosKhachHangVIP, lblPosKhachDiem;
		private Label lblPosGiamGiaTien, lblPosDiemDuyet;
		private NumericUpDown numPosDiemDoi;

		// Controls Kho Thuốc
		private DataGridView dgvKhoThuoc;
		private TextBox txtKhoSearch;

		// Controls Khách Hàng VIP
		private DataGridView dgvKhachHang;
		private TextBox txtKhachSearch;

		// Controls Báo Cáo
		private ComboBox cboBaoCaoThang;
		private NumericUpDown numBaoCaoNam;
		private DataGridView dgvTopKhachHang, dgvLichSuBaoCao;
		private Panel pnlChartDoanhThu;
		private DataGridView dgvBaoCaoLichSu;
		private Label lblBaoCaoDoanhThu, lblBaoCaoTongDon;
		private Label lblBaoCaoDoanhThuVal, lblBaoCaoDoanhThuSub;
		private Label lblBaoCaoTongDonVal, lblBaoCaoTongDonSub;
		private Label lblBaoCaoDoanhThuNamVal, lblBaoCaoDoanhThuNamSub;
		private Label lblCard1Value, lblCard1Sub;
		private Label lblCard2Value, lblCard2Sub;
		private Label lblCard3Value, lblCard3Sub;
		private Label lblCard4Value, lblCard4Sub;
		private Label lblTopTenThuoc;
		private Label lblTopChiTiet;
		private ComboBox cboPosDonViTinh;

		private bool isUpdatingSearchText = false; // Biến cờ ngăn đụng độ sự kiện khi click chọn dòng
		public Form1()
		{
			InitializeComponent();
			SetupDashboardUI();
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			// 1. Khởi tạo & Nạp dữ liệu vào bộ nhớ
			QuanLyNhaThuocData.KhoiTaoDuLieuApp();

			// 2. 🎯 KIỂM TRA HẠN BẢO LƯU VIP VÀ CẬP NHẬT ĐÚNG CHU KỲ 1 NĂM)
			CapNhatLaiToanBoHangVip();

			// 3. Nạp dữ liệu lên các bảng giao diện
			LoadKhachHangToPosGrid();

			string txtSearch = txtSearchKhachDoiQua?.Text?.Trim() ?? "";
			LoadKhachHangDoiQuaGrid(txtSearch, "");

			if (QuanLyQuaTangData.DanhSachQua != null)
			{
				LoadGridQuaTang(QuanLyQuaTangData.DanhSachQua);
			}
		}
		private void SetupDashboardUI()
		{
			this.Text = "Hệ Thống Quản Lý Nhà Thuốc & Tích Điểm VIP";
			this.ClientSize = new Size(1300, 780);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
			this.BackColor = Color.FromArgb(245, 247, 250);

			// 1. SIDEBAR
			sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(24, 43, 73) };
			Label lblLogo = new Label
			{
				Text = "💊 NHÀ THUỐC VIP",
				Font = new Font("Segoe UI", 13F, FontStyle.Bold),
				ForeColor = Color.White,
				Dock = DockStyle.Top,
				Height = 60,
				TextAlign = ContentAlignment.MiddleCenter
			};
			sidebarPanel.Controls.Add(lblLogo);

			Button btnBaoCao = CreateMenuButton("📊 Lịch Sử & Báo Cáo");
			Button btnDoiQua = CreateMenuButton("🎁 Đổi Quà VIP");
			Button btnKhachHang = CreateMenuButton("👥 Khách Hàng VIP");
			Button btnKhoThuoc = CreateMenuButton("💊 Quản Lý Kho Thuốc");
			Button btnBanHang = CreateMenuButton("🛒 Bán Hàng (POS)");

			btnBanHang.Click += (s, e) => SwitchMenu("🛒 QUẢN LÝ BÁN HÀNG & TẠO ĐƠN", btnBanHang, BuildPosView);
			btnKhoThuoc.Click += (s, e) => SwitchMenu("💊 QUẢN LÝ KHO THUỐC & TỒN KHO", btnKhoThuoc, BuildKhoThuocView);
			btnKhachHang.Click += (s, e) => SwitchMenu("👥 QUẢN LÝ KHÁCH HÀNG VIP", btnKhachHang, BuildKhachHangView);
			btnDoiQua.Click += (s, e) => SwitchMenu("🎁 ĐỔI QUÀ TẶNG BẰNG ĐIỂM TÍCH LŨY", btnDoiQua, BuildDoiQuaView);
			btnBaoCao.Click += (s, e) => SwitchMenu("📊 BÁO CÁO THỐNG KÊ & BẢNG XẾP HẠNG", btnBaoCao, BuildBaoCaoView);

			sidebarPanel.Controls.AddRange(new Control[] { btnBaoCao, btnDoiQua, btnKhachHang, btnKhoThuoc, btnBanHang });

			// 2. HEADER
			headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
			lblHeaderTitle = new Label
			{
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = Color.FromArgb(44, 62, 80),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(15, 0, 0, 0)
			};
			headerPanel.Controls.Add(lblHeaderTitle);

			// 3. MAIN CONTENT
			mainContentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(245, 247, 250) };

			this.Controls.AddRange(new Control[] { mainContentPanel, headerPanel, sidebarPanel });
			btnBanHang.PerformClick();
		}

		private Button CreateMenuButton(string text)
		{
			return new Button
			{
				Text = "   " + text,
				Dock = DockStyle.Top,
				Height = 50,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Color.LightGray,
				BackColor = Color.FromArgb(24, 43, 73),
				Font = new Font("Segoe UI", 10F, FontStyle.Regular),
				TextAlign = ContentAlignment.MiddleLeft,
				Cursor = Cursors.Hand
			};
		}

		private void SwitchMenu(string title, Button clickedButton, Action buildViewAction)
		{
			lblHeaderTitle.Text = title;
			foreach (Control ctrl in sidebarPanel.Controls)
			{
				if (ctrl is Button btn)
				{
					btn.BackColor = Color.FromArgb(24, 43, 73);
					btn.ForeColor = Color.LightGray;
					btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
				}
			}
			clickedButton.BackColor = Color.FromArgb(0, 122, 204);
			clickedButton.ForeColor = Color.White;
			clickedButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

			mainContentPanel.Controls.Clear();
			buildViewAction.Invoke();
		}

		private void StyleDataGridView(DataGridView dgv)
		{
			dgv.BorderStyle = BorderStyle.FixedSingle;
			dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dgv.BackgroundColor = Color.White;
			dgv.GridColor = Color.FromArgb(230, 235, 240);

			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 43, 73);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			dgv.ColumnHeadersHeight = 32;

			dgv.RowsDefaultCellStyle.BackColor = Color.White;
			dgv.RowsDefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
			dgv.RowsDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
			dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 235, 255);
			dgv.RowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(24, 43, 73);

			dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

			dgv.RowTemplate.Height = 28;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.MultiSelect = false;
			dgv.ReadOnly = true;
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.AllowUserToResizeRows = false;
			dgv.RowHeadersVisible = false;
			dgv.ScrollBars = ScrollBars.Vertical;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		}

		// =========================================================================
		// GIAO DIỆN BÁN HÀNG (POS)
		// =========================================================================
		// =========================================================================
		// GIAO DIỆN BÁN HÀNG (POS) - ĐÃ FIX CHUẨN NÚT THANH TOÁN & IN BILL
		// =========================================================================
		private void BuildPosView()
		{
			gioHang.Clear();
			selectedKhachHangPos = null;

			TableLayoutPanel pnlPosLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 1
			};
			pnlPosLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
			pnlPosLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
			pnlPosLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			// ---------------------------------------------------------------------
			// CỘT BÊN TRÁI (1. Tìm thuốc & 2. Giỏ hàng)
			// ---------------------------------------------------------------------
			TableLayoutPanel pnlLeftLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 2,
				Margin = new Padding(0, 0, 5, 0)
			};
			pnlLeftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));
			pnlLeftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));

			// 1. TÌM CHỌN THUỐC
			GroupBox gbThuoc = new GroupBox { Text = "🔍 1. TÌM & CHỌN THUỐC", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

			TableLayoutPanel tblThuocLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 2
			};
			tblThuocLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			tblThuocLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			Panel pnlSearchThuoc = new Panel { Dock = DockStyle.Fill };

			txtPosSearchThuoc = new TextBox { Location = new Point(5, 6), Height = 28, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
			txtPosSearchThuoc.TextChanged += (s, e) => LoadThuocToPosGrid();

			// Ô chọn Đơn Vị Tính (Hộp / Vỉ / Viên)
			cboPosDonViTinh = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Width = 70,
				Height = 28,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			cboPosDonViTinh.Items.AddRange(new object[] { "Hộp", "Vỉ", "Viên" });
			cboPosDonViTinh.SelectedIndex = 0;

			Label lblSL = new Label { Text = "SL:", Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			numPosSoLuong = new NumericUpDown { Anchor = AnchorStyles.Top | AnchorStyles.Right, Width = 55, Minimum = 1, Maximum = 1000, Value = 1, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };

			Button btnAddCart = new Button { Text = "➕ Thêm", Anchor = AnchorStyles.Top | AnchorStyles.Right, Width = 85, Height = 29, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnAddCart.Click += BtnAddCart_Click;
			Button btnXoaSearchThuoc = new Button
			{
				Text = "🧹 Xóa",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Width = 75,
				Height = 29,
				BackColor = Color.FromArgb(108, 117, 125),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
				Cursor = Cursors.Hand
			};

			btnXoaSearchThuoc.FlatAppearance.BorderSize = 0;

			btnXoaSearchThuoc.Click += (s, e) =>
			{
				// Xóa nội dung tìm kiếm
				txtPosSearchThuoc.Text = "";

				// Trả số lượng về 1
				if (numPosSoLuong != null)
					numPosSoLuong.Value = 1;

				// Đưa con trỏ lại ô tìm thuốc
				txtPosSearchThuoc.Focus();

				// TextChanged vốn đã tự gọi LoadThuocToPosGrid()
				// nhưng gọi lại để chắc chắn hiện toàn bộ thuốc
				LoadThuocToPosGrid();
			};
			btnXoaSearchThuoc.Location =
	new Point(pnlSearchThuoc.Width - 75, 5);

			btnAddCart.Location =
				new Point(pnlSearchThuoc.Width - 165, 5);

			numPosSoLuong.Location =
				new Point(pnlSearchThuoc.Width - 225, 6);

			lblSL.Location =
				new Point(pnlSearchThuoc.Width - 252, 10);

			cboPosDonViTinh.Location =
				new Point(pnlSearchThuoc.Width - 327, 6);

			txtPosSearchThuoc.Width =
				Math.Max(100, pnlSearchThuoc.Width - 337);
			pnlSearchThuoc.SizeChanged += (s, e) =>
			{
				btnXoaSearchThuoc.Location =
					new Point(pnlSearchThuoc.Width - 75, 5);

				btnAddCart.Location =
					new Point(pnlSearchThuoc.Width - 165, 5);

				numPosSoLuong.Location =
					new Point(pnlSearchThuoc.Width - 225, 6);

				lblSL.Location =
					new Point(pnlSearchThuoc.Width - 252, 10);

				cboPosDonViTinh.Location =
					new Point(pnlSearchThuoc.Width - 327, 6);

				txtPosSearchThuoc.Width =
					Math.Max(100, pnlSearchThuoc.Width - 337);
			};

			pnlSearchThuoc.Controls.AddRange(
				new Control[]
				{
		txtPosSearchThuoc,
		cboPosDonViTinh,
		lblSL,
		numPosSoLuong,
		btnAddCart,
		btnXoaSearchThuoc
				}
			);
			dgvPosThuoc = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvPosThuoc);
			dgvPosThuoc.SelectionChanged += DgvPosThuoc_SelectionChanged;
			tblThuocLayout.Controls.Add(pnlSearchThuoc, 0, 0);
			tblThuocLayout.Controls.Add(dgvPosThuoc, 0, 1);
			gbThuoc.Controls.Add(tblThuocLayout);

			pnlLeftLayout.Controls.Add(gbThuoc, 0, 0);

			// 2. GIỎ HÀNG THUỐC
			GroupBox gbGioHang = new GroupBox { Text = "🛒 2. GIỎ HÀNG THUỐC", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

			dgvPosGioHang = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvPosGioHang);

			Panel pnlBottomGioHang = new Panel { Dock = DockStyle.Bottom, Height = 95, Padding = new Padding(5) };

			Button btnXoaMon = new Button { Text = "❌ Xóa Món", Location = new Point(5, 5), Width = 100, Height = 32, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnXoaMon.Click += (s, e) => {
				if (dgvPosGioHang.CurrentRow != null && dgvPosGioHang.CurrentRow.Index < gioHang.Count)
				{ gioHang.RemoveAt(dgvPosGioHang.CurrentRow.Index); UpdatePosGioHangGrid(); }
			};

			lblPosTongTien = new Label { Text = "TỔNG HÀNG: 0đ  |  THU THỰC TẾ: 0đ", Location = new Point(115, 3), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.DarkRed };
			lblPosDiemQuyDoi = new Label { Text = "(Tích lũy đơn này: +0 điểm)", Location = new Point(115, 26), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.Blue };

			TableLayoutPanel pnlThanhToanLayout = new TableLayoutPanel
			{
				Location = new Point(5, 42),
				Size = new Size(pnlBottomGioHang.Width - 10, 48),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ColumnCount = 2,
				RowCount = 1
			};
			pnlThanhToanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
			pnlThanhToanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

			Button btnThanhToan = new Button
			{
				Text = "💳 XÁC NHẬN THANH TOÁN",
				Dock = DockStyle.Fill,
				BackColor = Color.FromArgb(40, 167, 69),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
				Margin = new Padding(0, 0, 4, 0)
			};
			btnThanhToan.Click += BtnThanhToan_Click;

			Button btnInBill = new Button
			{
				Text = "🖨️ IN BILL",
				Dock = DockStyle.Fill,
				BackColor = Color.FromArgb(0, 122, 204),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold)
			};
			btnInBill.Click += (s, e) =>
			{
				if (gioHang.Count == 0)
				{
					MessageBox.Show("Giỏ hàng đang trống!", "Thông báo");
					return;
				}

				// 1. Lấy khách hàng hiện tại đang chọn
				KhachHang kh = selectedKhachHangPos;

				// 2. Tính tổng tiền gốc từ List giỏ hàng
				decimal tongTienGoc = gioHang.Sum(x => x.ThanhTien);

				// 3. Lấy số điểm khách muốn dùng (từ ô NumericUpDown)
				int diemDaDung = (int)numPosDiemDoi.Value;
				decimal tienGiam = diemDaDung * 10; // 1 điểm = 10đ

				// 4. Tính toán số tiền thực tế phải thu
				decimal tongTienThucTe = tongTienGoc - tienGiam;
				if (tongTienThucTe < 0) tongTienThucTe = 0;

				// 5. Tính điểm cộng thêm cho đơn này theo HẠNG VIP đang được bảo lưu
				double heSoInBill = kh != null ? GetVipMultiplierByCap(kh.CapVip) : 1.0;
				int diemCongGocInBill = (int)(tongTienThucTe / 1000);
				int diemCongMoi = (int)(diemCongGocInBill * heSoInBill);

				// 6. Gọi hàm in và truyền ĐẦY ĐỦ tham số vào!
				InHoaDonPOS(dgvPosGioHang, kh, tongTienThucTe, diemCongMoi, diemDaDung);
			};
			pnlThanhToanLayout.Controls.Add(btnThanhToan, 0, 0);
			pnlThanhToanLayout.Controls.Add(btnInBill, 1, 0);

			pnlBottomGioHang.Controls.AddRange(new Control[] { btnXoaMon, lblPosTongTien, lblPosDiemQuyDoi, pnlThanhToanLayout });

			gbGioHang.Controls.Add(dgvPosGioHang);
			gbGioHang.Controls.Add(pnlBottomGioHang);

			pnlLeftLayout.Controls.Add(gbGioHang, 0, 1);

			// ---------------------------------------------------------------------
			// CỘT BÊN PHẢI (3. Tìm Khách Hàng & Quy Đổi Điểm VIP)
			// ---------------------------------------------------------------------
			GroupBox gbKhachHang = new GroupBox { Text = "👥 3. TÌM KHÁCH HÀNG  QUY ĐỔI ĐIỂM VIP", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

			TableLayoutPanel pnlRightLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 2,
				Padding = new Padding(3)
			};
			pnlRightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
			pnlRightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));

			Panel pnlSearchKhach = new Panel { Dock = DockStyle.Fill };
			Panel pnlSearchKhachTop = new Panel { Dock = DockStyle.Top, Height = 35 };
			Label lblSearchKhach = new Label { Text = "SĐT/Tên:", Location = new Point(0, 7), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };

			Button btnXoaKhachText = new Button
			{
				Text = "🧹 Xóa",
				Width = 65,
				Height = 28,
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = Color.FromArgb(108, 117, 125),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
				Cursor = Cursors.Hand
			};

			btnXoaKhachText.FlatAppearance.BorderSize = 0; 
			btnXoaKhachText.Click += (s, e) => { txtPosSearchKhach.Text = ""; txtPosSearchKhach.Focus(); };

			txtPosSearchKhach = new TextBox
			{
				Location = new Point(60, 4),
				Height = 28,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
			};

			// Nút Xóa nhỏ gọn bên phải
			btnXoaKhachText.Location =
				new Point(pnlSearchKhachTop.Width - 70, 3);

			// Chừa khoảng trống cho nút Xóa
			txtPosSearchKhach.Width =
				Math.Max(100, pnlSearchKhachTop.Width - 135);
			pnlSearchKhachTop.SizeChanged += (s, e) =>
			{
				btnXoaKhachText.Location =
					new Point(pnlSearchKhachTop.Width - 70, 3);

				txtPosSearchKhach.Width =
					Math.Max(100, pnlSearchKhachTop.Width - 135);
			};
			txtPosSearchKhach.Width = pnlSearchKhachTop.Width - 140;
			txtPosSearchKhach.TextChanged += (s, e) => { if (!isUpdatingSearch) LoadKhachHangToPosGrid(); };

			pnlSearchKhachTop.Controls.AddRange(new Control[] { lblSearchKhach, txtPosSearchKhach, btnXoaKhachText });

			dgvKhachHangPos = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvKhachHangPos);
			dgvKhachHangPos.SelectionChanged += DgvKhachHangPos_SelectionChanged;
			dgvKhachHangPos.CellClick += DgvKhachHangPos_CellClick;
			dgvKhachHangPos.CellFormatting += dgvKhachHangPos_CellFormatting;
			pnlSearchKhach.Controls.AddRange(new Control[] { dgvKhachHangPos, pnlSearchKhachTop });
			pnlRightLayout.Controls.Add(pnlSearchKhach, 0, 0);

			GroupBox gbSelectedCard = new GroupBox
			{
				Text = "📌 THẺ KHÁCH HÀNG  QUY ĐỔI ĐIỂM",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
				BackColor = Color.FromArgb(250, 252, 255)
			};

			Panel pnlQuyDinh = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 75,
				Padding = new Padding(10, 5, 10, 5),
				BackColor = Color.FromArgb(245, 247, 250)
			};
			Label lblQuyDinh = new Label
			{
				Text = "💡 Quy định VIP:\n• 1 Điểm = 10 VNĐ giảm trực tiếp vào đơn.\n• Giảm không quá 50% tổng đơn hàng.\n• Mua 1000 VNĐ được cộng 1 điểm.",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
				ForeColor = Color.DimGray
			};
			pnlQuyDinh.Controls.Add(lblQuyDinh);

			TableLayoutPanel tblCardContent = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 5,
				Padding = new Padding(10, 8, 10, 8)
			};
			tblCardContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
			tblCardContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
			tblCardContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
			tblCardContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
			tblCardContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			Panel pnlKhachInfo = new Panel { Dock = DockStyle.Fill };
			lblPosKhachTen = new Label { Text = "Khách hàng: Khách Lẻ (Chưa chọn)", Location = new Point(2, 2), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(24, 43, 73) };
			lblPosKhachHangVIP = new Label { Text = "Hạng thành viên: --", Location = new Point(2, 26), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.DimGray };
			lblPosKhachDiem = new Label { Text = "Tích lũy: 0 điểm (= 0 VNĐ)", Location = new Point(2, 48), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.DarkGreen };
			pnlKhachInfo.Controls.AddRange(new Control[] { lblPosKhachTen, lblPosKhachHangVIP, lblPosKhachDiem });

			Label lblLine = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(210, 215, 220) };

			Panel pnlInputDiem = new Panel { Dock = DockStyle.Fill };
			Label lblDiemDoi = new Label { Text = "Dùng điểm giảm:", Location = new Point(2, 6), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
			numPosDiemDoi = new NumericUpDown { Location = new Point(125, 3), Width = 110, Minimum = 0, Maximum = 0, Increment = 100, Enabled = false, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.DarkRed };
			numPosDiemDoi.ValueChanged += (s, e) => CapNhatGioiHanDiemPos();
			pnlInputDiem.Controls.AddRange(new Control[] { lblDiemDoi, numPosDiemDoi });

			TableLayoutPanel pnlQuickButtons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
			pnlQuickButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));
			pnlQuickButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23F));
			pnlQuickButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
			pnlQuickButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));

			Button btn100 = new Button { Text = "+100", Dock = DockStyle.Fill, BackColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = new Padding(1) };
			Button btn500 = new Button { Text = "+500", Dock = DockStyle.Fill, BackColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = new Padding(1) };
			Button btn1000 = new Button { Text = "+1,000", Dock = DockStyle.Fill, BackColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = new Padding(1) };
			Button btnMax = new Button { Text = "Tối đa", Dock = DockStyle.Fill, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = new Padding(1) };

			btn100.Click += (s, e) => AddDiemDoi(100);
			btn500.Click += (s, e) => AddDiemDoi(500);
			btn1000.Click += (s, e) => AddDiemDoi(1000);
			btnMax.Click += (s, e) => { if (numPosDiemDoi.Enabled) numPosDiemDoi.Value = numPosDiemDoi.Maximum; };

			pnlQuickButtons.Controls.Add(btn100, 0, 0);
			pnlQuickButtons.Controls.Add(btn500, 1, 0);
			pnlQuickButtons.Controls.Add(btn1000, 2, 0);
			pnlQuickButtons.Controls.Add(btnMax, 3, 0);

			Panel pnlSummary = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 4), BackColor = Color.FromArgb(235, 245, 255), BorderStyle = BorderStyle.FixedSingle };
			lblPosGiamGiaTien = new Label { Text = "💰 GIẢM GIÁ: -0 VNĐ", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.DarkRed };
			lblPosDiemDuyet = new Label { Text = "Trừ tích lũy: -0 điểm", Location = new Point(10, 36), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.FromArgb(0, 100, 200) };
			pnlSummary.Controls.AddRange(new Control[] { lblPosGiamGiaTien, lblPosDiemDuyet });

			tblCardContent.Controls.Add(pnlKhachInfo, 0, 0);
			tblCardContent.Controls.Add(lblLine, 0, 1);
			tblCardContent.Controls.Add(pnlInputDiem, 0, 2);
			tblCardContent.Controls.Add(pnlQuickButtons, 0, 3);
			tblCardContent.Controls.Add(pnlSummary, 0, 4);

			gbSelectedCard.Controls.Add(tblCardContent);
			gbSelectedCard.Controls.Add(pnlQuyDinh);

			pnlRightLayout.Controls.Add(gbSelectedCard, 0, 1);

			gbKhachHang.Controls.Add(pnlRightLayout);

			pnlPosLayout.Controls.Add(pnlLeftLayout, 0, 0);
			pnlPosLayout.Controls.Add(gbKhachHang, 1, 0);

			mainContentPanel.Controls.Add(pnlPosLayout);

			LoadThuocToPosGrid();
			LoadKhachHangToPosGrid();
		}
		// Hàm tính tiền giảm giá & tổng đơn hàng POS
		private void CapNhatGioiHanDiemPos()
		{
			decimal tongTienHang = gioHang.Sum(g => g.ThanhTien);

			if (selectedKhachHangPos != null && tongTienHang > 0)
			{
				// Chỉ dùng ĐIỂM KHẢ DỤNG để giảm tiền.
				// Không fallback sang DiemTichLuy vì điểm xét hạng không phải ví điểm.
				int diemKhachCo = selectedKhachHangPos.DiemKhaDung;
				int diemToiDaQuyDinh = (int)((tongTienHang * 0.5m) / 10); // Khống chế tối đa 50% đơn

				numPosDiemDoi.Enabled = true;
				numPosDiemDoi.Maximum = Math.Max(0, Math.Min(diemKhachCo, diemToiDaQuyDinh));

				if (numPosDiemDoi.Value > numPosDiemDoi.Maximum)
					numPosDiemDoi.Value = numPosDiemDoi.Maximum;
			}
			else
			{
				numPosDiemDoi.Value = 0;
				numPosDiemDoi.Maximum = 0;
				numPosDiemDoi.Enabled = false;
			}

			int diemTru = (int)numPosDiemDoi.Value;
			decimal tienGiam = diemTru * 10;
			decimal tienPhaiTra = Math.Max(0, tongTienHang - tienGiam);

			// Hiển thị tạm theo hệ số hạng hiện tại để khớp số điểm thực tế khi thanh toán.
			double heSoVip = selectedKhachHangPos != null
				? GetVipMultiplierByCap(selectedKhachHangPos.CapVip)
				: 1.0;

			int diemCongGoc = (int)(tienPhaiTra / 1000);
			int diemCong = (int)(diemCongGoc * heSoVip);

			if (lblPosTongTien != null)
				lblPosTongTien.Text = $"TỔNG HÀNG: {tongTienHang:N0}đ  |  THU THỰC TẾ: {tienPhaiTra:N0}đ";

			if (lblPosDiemQuyDoi != null)
				lblPosDiemQuyDoi.Text = $"(Tích lũy đơn này: +{diemCong:N0} điểm)";

			if (lblPosGiamGiaTien != null)
				lblPosGiamGiaTien.Text = $"💰 GIẢM GIÁ: -{tienGiam:N0} VNĐ";

			if (lblPosDiemDuyet != null)
				lblPosDiemDuyet.Text = $"Trừ tích lũy: -{diemTru:N0} điểm";
		}

		private void UpdatePosGioHangGrid()
		{
			dgvPosGioHang.DataSource = gioHang.Select((g, i) => new {
				STT = i + 1,
				Tên_Thuốc = g.ThuocItem.TenThuoc,
				ĐVT = g.DonViChon,
				SL = g.SoLuong,
				Đơn_Giá = g.DonGia.ToString("N0"),
				Thành_Tiền = g.ThanhTien.ToString("N0")
			}).ToList();

			CapNhatGioiHanDiemPos();
		}
		private void AddDiemDoi(int amount)
		{
			if (numPosDiemDoi != null && numPosDiemDoi.Enabled)
			{
				decimal target = numPosDiemDoi.Value + amount;
				numPosDiemDoi.Value = Math.Min(numPosDiemDoi.Maximum, target);
			}
		}
		private void DgvKhachHangPos_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < dgvKhachHangPos.Rows.Count)
			{
				string sdt = dgvKhachHangPos.Rows[e.RowIndex].Cells["SĐT"].Value?.ToString();
				if (!string.IsNullOrEmpty(sdt)) { isUpdatingSearch = true; txtPosSearchKhach.Text = sdt; isUpdatingSearch = false; }
			}
		}
		private void LoadThuocToPosGrid()
		{
			string kw = txtPosSearchThuoc.Text.Trim().ToLower();

			var ds = QuanLyNhaThuocData.DanhSachThuoc
				.Where(t => string.IsNullOrEmpty(kw) ||
							t.MaThuoc.ToLower().Contains(kw) ||
							t.TenThuoc.ToLower().Contains(kw) ||
							t.ThanhPhan.ToLower().Contains(kw))
				.Select(t => new
				{
					Mã = t.MaThuoc,
					Tên_Thuốc = t.TenThuoc,
					Thành_Phần = t.ThanhPhan,
					ĐVT = t.DonViTinh,
					Giá = t.GiaBan.ToString("N0"),
					Tồn = t.TonKhoHienThi
				}).ToList();

			dgvPosThuoc.DataSource = ds;
			DgvPosThuoc_SelectionChanged(null, null);
		}
		private void LoadKhachHangToPosGrid()
		{
			if (QuanLyNhaThuocData.DanhSachKhachHang == null) return;

			// Kiểm tra hạn bảo lưu hạng. Nếu có thay đổi thì lưu ngay xuống file 5 cột.
			bool coThayDoiVip = false;
			foreach (var kh in QuanLyNhaThuocData.DanhSachKhachHang)
			{
				if (kh != null && kh.KiemTraCapNhatHangVipLongChau())
					coThayDoiVip = true;
			}

			if (coThayDoiVip)
				QuanLyNhaThuocData.LuuFileKhachHang();

			string kw = txtPosSearchKhach?.Text?.Trim().ToLower() ?? "";

			dgvKhachHangPos.DataSource = null;
			dgvKhachHangPos.DataSource = QuanLyNhaThuocData.DanhSachKhachHang
				.Where(k => k != null && (string.IsNullOrEmpty(kw) ||
							(k.SoDienThoai != null && k.SoDienThoai.Contains(kw)) ||
							(k.HoTen != null && k.HoTen.ToLower().Contains(kw))))
				.Select(k => new {
					SĐT = k.SoDienThoai ?? "",
					Họ_Tên = k.HoTen ?? "",
					Điểm = k.DiemKhaDung,
					// Hạng đang được bảo lưu, không suy lại từ số dư điểm.
					Hạng = k.HangVip
				}).ToList();
		}

		private void dgvKhachHangPos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
			{
				string colName = dgvKhachHangPos.Columns[e.ColumnIndex].Name;
				if (colName == "Hạng" || colName.Contains("Hạng"))
				{
					// Lấy chữ Hạng đang hiển thị sẵn trên ô (Ví dụ: "VIP Vàng")
					string tenHang = e.Value?.ToString() ?? "";

					// CHỈ TÔ MÀU THEO CHỮ ĐÓ - KHÔNG ĐỔI CHỮ, KHÔNG TÍNH ĐIỂM
					e.CellStyle.ForeColor = GetColorByTierName(tenHang);
					e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
				}
			}
		}
		private void CapNhatLaiToanBoHangVip()
		{
			if (QuanLyNhaThuocData.DanhSachKhachHang == null ||
				QuanLyNhaThuocData.DanhSachKhachHang.Count == 0)
				return;

			bool coThayDoi = false;

			foreach (var kh in QuanLyNhaThuocData.DanhSachKhachHang)
			{
				if (kh == null) continue;

				// Chỉ để KhachHang tự xét theo chu kỳ bảo lưu 1 năm.
				// Không ghi đè HangVip từ DiemTichLuy/DiemXetHang365Ngay nữa.
				if (kh.KiemTraCapNhatHangVipLongChau())
					coThayDoi = true;
			}

			// Nếu hạ hạng / giữ chu kỳ mới / thăng hạng thì lưu ngay CapVip + NgayThangHang.
			if (coThayDoi)
				QuanLyNhaThuocData.LuuFileKhachHang();
		}
		private (string Name, Color Color) GetVipTier(int diem)
		{
			// Tông màu đậm, độ tương phản cao trên nền trắng
			if (diem >= 5000) return ("💎 VIP Kim Cương", Color.FromArgb(0, 102, 204));  // Xanh da trời ngọc
			if (diem >= 2000) return ("👑 VIP Vàng", Color.DarkOrange);      // Vàng đồng đậm (đặc, rõ)
			if (diem >= 500) return ("🥈 VIP Bạc", Color.Gray);       // Xám đá/Xám thép đậm
			return ("🌱 Thành Viên", Color.FromArgb(80, 80, 80));                       // Xám than đậm
		}

		// Lấy giao diện hạng từ CẤP VIP đang được bảo lưu.
		private (string Name, Color Color) GetVipTierByCap(int cap)
		{
			switch (cap)
			{
				case 4:
					return ("💎 VIP Kim Cương", Color.FromArgb(0, 102, 204));
				case 3:
					return ("👑 VIP Vàng", Color.DarkOrange);
				case 2:
					return ("🥈 VIP Bạc", Color.Gray);
				default:
					return ("🌱 Thành Viên", Color.FromArgb(80, 80, 80));
			}
		}

		// Hệ số tích điểm dựa trên HẠNG ĐANG ĐƯỢC BẢO LƯU,
		// không dựa trên số dư điểm hoặc điểm 365 ngày.
		private double GetVipMultiplierByCap(int cap)
		{
			switch (cap)
			{
				case 4: return 2.0;
				case 3: return 1.5;
				case 2: return 1.2;
				default: return 1.0;
			}
		}
		private (string NextTierName, int PointsNeeded)
	GetNextTierInfo(KhachHang kh)
		{
			if (kh == null)
				return ("", 0);

			// Điểm dùng xét thăng hạng:
			// là điểm kiếm được do mua hàng trong chu kỳ VIP,
			// KHÔNG phải số dư điểm còn lại để tiêu.
			int diemXetVip =
				kh.DiemVipChuKyHienTai;


			switch (kh.CapVip)
			{
				case 1:
					return (
						"VIP Bạc",
						Math.Max(
							0,
							500 - diemXetVip
						)
					);


				case 2:
					return (
						"VIP Vàng",
						Math.Max(
							0,
							2000 - diemXetVip
						)
					);


				case 3:
					return (
						"VIP Kim Cương",
						Math.Max(
							0,
							5000 - diemXetVip
						)
					);


				default:
					return (
						"Kim Cương (Đã đạt mốc cao nhất)",
						0
					);
			}
		}

		private void CapNhatGoiYVip(KhachHang kh)
		{
			if (lblVipHint == null || kh == null) return;

			if (kh.CapVip >= 4)
			{
				lblVipHint.Text =
					$"💎 VIP Kim Cương | Duy trì: {kh.DiemVipChuKyHienTai:N0}/{kh.DiemCanDuyTriHangHienTai:N0} điểm | " +
					$"Hạn: {kh.HanGiuHang:dd/MM/yyyy}";
				lblVipHint.ForeColor = Color.DarkGreen;
				lblVipHint.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
				return;
			}

			var next = GetNextTierInfo(kh);

			if (next.PointsNeeded > 0)
			{
				lblVipHint.Text =
					$"💡 Còn thiếu {next.PointsNeeded:N0} điểm trong chu kỳ để thăng hạng [{next.NextTierName}] | " +
					$"Hạn hạng hiện tại: {kh.HanGiuHang:dd/MM/yyyy}";
				lblVipHint.ForeColor = Color.DarkOrange;
				lblVipHint.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			}
			else
			{
				lblVipHint.Text = "✅ Đã đủ điều kiện xét thăng hạng!";
				lblVipHint.ForeColor = Color.DarkGreen;
				lblVipHint.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			}
		}
		private void BtnAddCart_Click(object sender, EventArgs e)
		{
			if (dgvPosThuoc.CurrentRow == null)
			{
				MessageBox.Show("Vui lòng chọn thuốc cần thêm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string ma = dgvPosThuoc.CurrentRow.Cells["Mã"].Value?.ToString();
			var thuoc = QuanLyNhaThuocData.DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == ma);
			if (thuoc == null) return;

			string dvtChon = cboPosDonViTinh.SelectedItem?.ToString() ?? "Hộp";
			int slMuonMua = (int)numPosSoLuong.Value;

			var monMoi = new ChiTietGioHang { ThuocItem = thuoc, DonViChon = dvtChon, SoLuong = slMuonMua };

			int tongVienDaCoTrongGio = gioHang.Where(g => g.ThuocItem.MaThuoc == ma).Sum(g => g.TongSoVienCanTru);
			if (tongVienDaCoTrongGio + monMoi.TongSoVienCanTru > thuoc.SoLuongTonVien)
			{
				MessageBox.Show($"Kho không đủ hàng!\nTồn kho hiện tại: {thuoc.TonKhoHienThi}", "Cảnh báo kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var exist = gioHang.FirstOrDefault(g => g.ThuocItem.MaThuoc == ma && g.DonViChon == dvtChon);
			if (exist != null)
				exist.SoLuong += slMuonMua;
			else
				gioHang.Add(monMoi);

			UpdatePosGioHangGrid();
		}

		private void BtnThanhToan_Click(object sender, EventArgs e)
		{
			if (gioHang == null || gioHang.Count == 0)
			{
				MessageBox.Show("Giỏ hàng đang trống!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			decimal tongTienHang = gioHang.Sum(g => g.ThanhTien);
			int diemTru = (int)numPosDiemDoi.Value;
			decimal tienGiam = diemTru * 10;
			decimal tienPhaiTra = Math.Max(0, tongTienHang - tienGiam);

			// 🎯 1. LẤY HẠNG ĐANG ĐƯỢC BẢO LƯU TRƯỚC KHI THANH TOÁN
			double heSoVip = 1.0;
			var tierCu = GetVipTierByCap(1);
			var tierMoi = GetVipTierByCap(1);
			bool coThangHang = false;
			int capVipCu = 1;

			if (selectedKhachHangPos != null)
			{
				// Xét hạn trước khi áp dụng quyền lợi cho đơn hiện tại.
				if (selectedKhachHangPos.KiemTraCapNhatHangVipLongChau())
					QuanLyNhaThuocData.LuuFileKhachHang();

				capVipCu = selectedKhachHangPos.CapVip;
				tierCu = GetVipTierByCap(capVipCu);
				heSoVip = GetVipMultiplierByCap(capVipCu);
			}

			// Tính điểm cộng dựa trên hệ số của HẠNG ĐANG ĐƯỢC BẢO LƯU
			int diemCongGoc = (int)(tienPhaiTra / 1000);
			int diemCong = (int)(diemCongGoc * heSoVip);

			string sdt = selectedKhachHangPos != null
				? selectedKhachHangPos.SoDienThoai
				: "KHACH-LE";

			// Trừ số lượng tồn kho thuốc
			foreach (var item in gioHang)
			{
				item.ThuocItem.SoLuongTonVien =
					Math.Max(0, item.ThuocItem.SoLuongTonVien - item.TongSoVienCanTru);
			}
			QuanLyNhaThuocData.LuuFileThuoc();

			// Tạo thông tin đơn hàng
			// Lưu từng mặt hàng riêng bằng dấu ;
			// Đồng thời lưu rõ ĐVT và số lượng.
			//
			// Ví dụ:
			// Paracetamol 500mg [Hộp] x1;
			// Paracetamol 500mg [Vỉ] x1;
			// Berocca Performance [Tuýp] x1
			string chiTietStr = string.Join(
				";",
				gioHang.Select(g =>
					$"{g.ThuocItem.TenThuoc} [{g.DonViChon}] x{g.SoLuong}"
				)
			);

			var donHang = new LichSuMuaHang
			{
				MaHoaDon = "HD" + (QuanLyNhaThuocData.DanhSachDonHang.Count + 1).ToString("D3"),
				NgayMua = DateTime.Now,
				SoDienThoai = sdt,
				TongTien = tienPhaiTra,
				DiemCong = diemCong,
				DiemTru = diemTru,
				ChiTietDonHang = chiTietStr
			};

			QuanLyNhaThuocData.DanhSachDonHang.Add(donHang);
			QuanLyNhaThuocData.LuuFileDonHang();

			// 🎯 2. CẬP NHẬT LỊCH SỬ VÀ ĐIỂM CHO KHÁCH HÀNG
			if (selectedKhachHangPos != null)
			{
				// Trừ ví điểm khả dụng nếu khách chọn đổi điểm giảm giá.
				// Việc tiêu điểm KHÔNG làm thay đổi CapVip / NgayThangHang.
				if (diemTru > 0)
					selectedKhachHangPos.TruDiem(diemTru);

				if (selectedKhachHangPos.DanhSachLichSu == null)
					selectedKhachHangPos.DanhSachLichSu = new List<LichSuMuaHang>();

				// Phải thêm đơn vào lịch sử trước khi xét thăng hạng
				// để điểm của chính đơn này được tính vào chu kỳ VIP.
				selectedKhachHangPos.DanhSachLichSu.Add(donHang);

				if (diemCong > 0)
					selectedKhachHangPos.CongDiem(diemCong);

				// CongDiem đã gọi xét VIP, gọi lại vẫn an toàn.
				selectedKhachHangPos.KiemTraCapNhatHangVipLongChau();

				tierMoi = GetVipTierByCap(selectedKhachHangPos.CapVip);

				// Chỉ thông báo thăng hạng khi CẤP VIP thực sự tăng.
				coThangHang = selectedKhachHangPos.CapVip > capVipCu;

				QuanLyNhaThuocData.LuuFileKhachHang();
			}

			QuanLyNhaThuocData.LuuFileDonHang();

			// 🎯 3. THÔNG BÁO KẾT QUẢ
			string thongBaoDiem = heSoVip > 1.0
				? $"+{diemCong:N0} điểm (Đã x{heSoVip} ưu đãi {tierCu.Name})"
				: $"+{diemCong:N0} điểm";

			MessageBox.Show(
				$"Thanh toán thành công!\n" +
				$"Số tiền thu: {tienPhaiTra:N0} VNĐ\n" +
				$"Được giảm giá: -{tienGiam:N0} VNĐ\n" +
				$"Tích lũy đơn này: {thongBaoDiem}",
				"Thông Báo",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
			);

			if (coThangHang && selectedKhachHangPos != null)
			{
				MessageBox.Show(
					$"🎉 CHÚC MỪNG KHÁCH HÀNG: {selectedKhachHangPos.HoTen.ToUpper()}\n\n" +
					$"Khách hàng đã chính thức thăng hạng từ [{tierCu.Name}] ➔ [{tierMoi.Name}]!\n" +
					$"Hạng mới được bảo lưu đến: {selectedKhachHangPos.HanGiuHang:dd/MM/yyyy}\n" +
					$"Ưu đãi mới: Tích điểm x{GetVipMultiplierByCap(selectedKhachHangPos.CapVip)} cho mọi đơn hàng tiếp theo.",
					"🌟 THĂNG HẠNG VIP THÀNH CÔNG",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information
				);
			}

			// Clear giỏ hàng & nạp lại UI
			gioHang.Clear();
			if (numPosDiemDoi != null) numPosDiemDoi.Value = 0;

			UpdatePosGioHangGrid();
			LoadThuocToPosGrid();
			LoadKhachHangToPosGrid();

			string txtSearch = txtSearchKhachDoiQua?.Text?.Trim() ?? "";
			LoadKhachHangDoiQuaGrid(txtSearch, sdt);
			CapNhatTopThuocBanChay();
		}

		private double GetVipMultiplier(int diem)
		{
			if (diem >= 5000) return 2.0; // VIP Kim Cương: Tích điểm x2.0
			if (diem >= 2000) return 1.5; // VIP Vàng: Tích điểm x1.5
			if (diem >= 500) return 1.2; // VIP Bạc: Tích điểm x1.2
			return 1.0;                   // Khách thường / Mới: Tích điểm x1.0
		}
		private void DgvPosThuoc_SelectionChanged(object sender, EventArgs e)
		{
			if (dgvPosThuoc.CurrentRow == null || cboPosDonViTinh == null) return;

			string ma = dgvPosThuoc.CurrentRow.Cells["Mã"]?.Value?.ToString();
			var thuoc = QuanLyNhaThuocData.DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == ma);
			if (thuoc == null) return;

			cboPosDonViTinh.Items.Clear();
			cboPosDonViTinh.Items.Add(thuoc.DonViTinh);

			if (thuoc.SoViTrongHop > 1 || thuoc.GiaVi > 0)
			{
				cboPosDonViTinh.Items.Add("Vỉ");
			}

			if (thuoc.SoVienTrongVi > 1 || thuoc.GiaVien > 0)
			{
				cboPosDonViTinh.Items.Add("Viên");
			}

			cboPosDonViTinh.SelectedIndex = 0;
		}
		// Đã thêm các tham số: KhachHang, Tổng Tiền, Điểm Cộng, Điểm Trừ
		private void InHoaDonPOS(DataGridView dgvGioHang, KhachHang kh = null, decimal tongTienBill = 0, int diemCong = 0, int diemTru = 0)
		{
			if (dgvGioHang.Rows.Count == 0)
			{
				MessageBox.Show("Giỏ hàng đang trống, không có gì để in!", "Thông báo");
				return;
			}

			PrintDocument pd = new PrintDocument();
			pd.PrintPage += (sender, e) =>
			{
				Graphics g = e.Graphics;
				Font fontTitle = new Font("Courier New", 16, FontStyle.Bold);
				Font fontBold = new Font("Courier New", 10, FontStyle.Bold);
				Font fontRegular = new Font("Courier New", 10, FontStyle.Regular);
				Brush brush = Brushes.Black;

				int y = 20;
				int margin = 20;

				// 1. Header nhà thuốc
				g.DrawString("NHÀ THUỐC SIÊU NHÂN", fontTitle, brush, margin + 20, y);
				y += 30;
				g.DrawString("ĐC: 123 Đường C#, Lập Trình Viên", fontRegular, brush, margin, y); y += 20;
				g.DrawString("SĐT: 0909.888.999", fontRegular, brush, margin, y); y += 20;
				g.DrawString("Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRegular, brush, margin, y); y += 20;

				// [MỚI] THÊM THÔNG TIN KHÁCH HÀNG VIP
				if (kh != null)
				{
					g.DrawString($"Khách hàng: {kh.HoTen}", fontRegular, brush, margin, y); y += 20;
					g.DrawString($"Hạng: {kh.HangVip ?? "Thành Viên"}", fontRegular, brush, margin, y); y += 20;
				}

				g.DrawString("--------------------------------------", fontRegular, brush, margin, y);
				y += 20;

				// 2. Tiêu đề cột
				g.DrawString("Tên thuốc", fontBold, brush, margin, y);
				g.DrawString("ĐVT", fontBold, brush, margin + 155, y);
				g.DrawString("SL", fontBold, brush, margin + 205, y);
				g.DrawString("T.Tiền", fontBold, brush, margin + 245, y);
				y += 25;

				// 3. In từng mặt hàng trong giỏ
				decimal tongTienTinhToan = 0;

				foreach (DataGridViewRow row in dgvGioHang.Rows)
				{
					if (row.IsNewRow)
						continue;


					// =====================================================
					// LẤY THÔNG TIN TỪ GIỎ HÀNG
					// =====================================================
					string ten =
						row.Cells["Tên_Thuốc"].Value?.ToString()
						?? "";

					string dvt =
						row.Cells["ĐVT"].Value?.ToString()
						?? "";

					string sl =
						row.Cells["SL"].Value?.ToString()
						?? "0";


					string strTien =
						row.Cells["Thành_Tiền"].Value?.ToString()
						?? "0";


					decimal.TryParse(
						strTien
							.Replace(",", "")
							.Replace(".", "")
							.Replace("đ", "")
							.Trim(),
						out decimal tienRow
					);


					tongTienTinhToan += tienRow;


					// =====================================================
					// FORMAT TIỀN
					// =====================================================
					string tienIn =
					tienRow.ToString("N0") + "đ";

					// Rút gọn tên để khỏi đè sang cột ĐVT
					if (ten.Length > 14)
					{
						ten =
							ten.Substring(0, 14) + "..";
					}


					// =====================================================
					// IN TỪNG CỘT
					// =====================================================
					g.DrawString(
						ten,
						fontRegular,
						brush,
						margin,
						y
					);


					g.DrawString(
						dvt,
						fontRegular,
						brush,
						margin + 155,
						y
					);


					g.DrawString(
						sl,
						fontRegular,
						brush,
						margin + 207,
						y
					);


					g.DrawString(
						tienIn,
						fontRegular,
						brush,
						margin + 245,
						y
					);


					y += 20;
				}

				// =============================================================
				// TÍNH TIỀN HÓA ĐƠN
				// =============================================================

				// Giá gốc của toàn bộ thuốc, CHƯA trừ điểm
				decimal tongTienGoc = tongTienTinhToan;

				// Tiền được giảm từ điểm
				decimal tienGiamTuDiem = diemTru * 10m;

				// Thành tiền khách thực tế phải trả
				decimal thanhTien = tongTienGoc - tienGiamTuDiem;

				if (thanhTien < 0)
					thanhTien = 0;


				// =============================================================
				// HIỂN THỊ TỔNG TIỀN
				// =============================================================
				y += 10;

				g.DrawString(
					"--------------------------------------",
					fontRegular,
					brush,
					margin,
					y
				);

				y += 25;


				// 1. TỔNG CỘNG = GIÁ GỐC
				g.DrawString(
					"TỔNG CỘNG:",
					fontBold,
					brush,
					margin,
					y
				);

				g.DrawString(
					tongTienGoc.ToString("N0") + " VNĐ",
					fontBold,
					brush,
					margin + 180,
					y
				);

				y += 22;


				// 2. ĐIỂM ĐÃ DÙNG
				if (diemTru > 0)
				{
					g.DrawString(
						$"Đã dùng: -{diemTru:N0} điểm",
						fontRegular,
						brush,
						margin,
						y
					);

					g.DrawString(
						$"-{tienGiamTuDiem:N0} VNĐ",
						fontRegular,
						brush,
						margin + 180,
						y
					);

					y += 22;
				}


				// 3. THÀNH TIỀN = TIỀN SAU KHI TRỪ ĐIỂM
				g.DrawString(
					"THÀNH TIỀN:",
					fontBold,
					brush,
					margin,
					y
				);

				g.DrawString(
					thanhTien.ToString("N0") + " VNĐ",
					fontBold,
					brush,
					margin + 180,
					y
				);

				y += 25;


				// 4. ĐIỂM ĐƯỢC CỘNG TỪ ĐƠN NÀY
				if (diemCong > 0)
				{
					g.DrawString(
						$"Tích lũy thẻ: +{diemCong:N0} điểm",
						fontRegular,
						brush,
						margin,
						y
					);

					y += 20;
				}


				// 5. ĐIỂM KHẢ DỤNG
				if (kh != null)
				{
					g.DrawString(
						$"Điểm khả dụng hiện tại: {kh.DiemKhaDung:N0}",
						fontRegular,
						brush,
						margin,
						y
					);

					y += 20;
				}
				y += 15;

				g.DrawString(
					"CẢM ƠN QUÝ KHÁCH!",
					fontBold,
					brush,
					margin + 55,
					y
				);

				y += 22;

				g.DrawString(
					"Hẹn gặp lại quý khách!",
					fontRegular,
					brush,
					margin + 45,
					y
				);
			};

			PrintPreviewDialog previewDialog = new PrintPreviewDialog
			{
				Document = pd,
				Width = 450,
				Height = 600,
				Text = "Xem trước Hóa Đơn",
				ShowIcon = false
			};
			previewDialog.ShowDialog();
		}
		private void DgvKhachHangPos_SelectionChanged(object sender, EventArgs e)
		{
			// 🎯 BỌC AN TOÀN TRÁNH BỊ CHẠY LỖI NULL VÀ INDEX -1
			if (dgvKhachHangPos == null || dgvKhachHangPos.CurrentRow == null || dgvKhachHangPos.CurrentRow.Index < 0)
				return;

			string sdt = dgvKhachHangPos.CurrentRow.Cells["SĐT"].Value?.ToString();
			selectedKhachHangPos = QuanLyNhaThuocData.DanhSachKhachHang?
				.FirstOrDefault(k => k.SoDienThoai == sdt);

			if (selectedKhachHangPos != null)
			{
				lblPosKhachTen.Text = $"Khách hàng: {selectedKhachHangPos.HoTen}";

				// Hạng hiện tại lấy từ CapVip đang được bảo lưu.
				var tier = GetVipTierByCap(selectedKhachHangPos.CapVip);
				var next = GetNextTierInfo(selectedKhachHangPos);

				int diemTieuDung = selectedKhachHangPos.DiemKhaDung;
				int diemTrongKy = selectedKhachHangPos.DiemVipChuKyHienTai;

				if (selectedKhachHangPos.CapVip >= 4)
				{
					lblPosKhachHangVIP.Text =
						$"Hạng: {tier.Name} | Hạn: {selectedKhachHangPos.HanGiuHang:dd/MM/yyyy}";
				}
				else
				{
					lblPosKhachHangVIP.Text =
						$"Hạng: {tier.Name} (💡 Thiếu {next.PointsNeeded:N0} điểm ➔ {next.NextTierName}) | " +
						$"Hạn: {selectedKhachHangPos.HanGiuHang:dd/MM/yyyy}";
				}

				lblPosKhachHangVIP.ForeColor = tier.Color;

				// Điểm khả dụng là ví điểm; điểm kỳ VIP chỉ để xét thăng/duy trì.
				lblPosKhachDiem.Text =
					$"Tích lũy: {diemTieuDung:N0} điểm (= {diemTieuDung * 10:N0} VNĐ) | " +
					$"Điểm kỳ VIP: {diemTrongKy:N0}";
			}
			else
			{
				lblPosKhachTen.Text = "Khách hàng: Khách Lẻ (Chưa chọn)";
				lblPosKhachHangVIP.Text = "Hạng thành viên: --";
				lblPosKhachHangVIP.ForeColor = Color.DimGray;
				lblPosKhachDiem.Text = "Tích lũy: 0 điểm (= 0 VNĐ)";
			}

			UpdatePosGioHangGrid();
		}
		private Color GetColorByTierName(string tenHang)
		{
			if (string.IsNullOrEmpty(tenHang)) return Color.FromArgb(70, 80, 95);

			if (tenHang.Contains("Kim Cương")) return Color.FromArgb(0, 122, 255);   // Xanh Kim Cương
			if (tenHang.Contains("Vàng")) return Color.FromArgb(218, 165, 32);  // Vàng VIP
			if (tenHang.Contains("Bạc")) return Color.FromArgb(108, 117, 125); // Bạc

			return Color.FromArgb(70, 80, 95); // Mặc định: Thành Viên
		}
		private void dgvKhachHang_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
			{
				string colName = dgvKhachHang.Columns[e.ColumnIndex].Name;
				if (colName == "Hạng_VIP" || colName.Contains("Hạng"))
				{
					// Lấy chữ Hạng đang hiển thị sẵn trên ô
					string tenHang = e.Value?.ToString() ?? "";

					// CHỈ TÔ MÀU THEO CHỮ ĐÓ - KHÔNG ĐỔI CHỮ, KHÔNG TÍNH ĐIỂM
					e.CellStyle.ForeColor = GetColorByTierName(tenHang);
					e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
				}
			}
		}

		private void CapNhatTopThuocBanChay()
		{
			if (lblTopTenThuoc == null || lblTopChiTiet == null)
				return;

			if (QuanLyNhaThuocData.DanhSachDonHang == null)
				return;


			// ============================================================
			// CHỈ THỐNG KÊ HÓA ĐƠN BÁN HÀNG TRONG THÁNG HIỆN TẠI
			// Không tính đơn đổi quà DQ...
			// ============================================================
			int thangHienTai = DateTime.Now.Month;
			int namHienTai = DateTime.Now.Year;

			var danhSachDonTrongThang =
				QuanLyNhaThuocData.DanhSachDonHang
				.Where(d =>
					d != null &&
					d.NgayMua.Month == thangHienTai &&
					d.NgayMua.Year == namHienTai &&
					!string.IsNullOrWhiteSpace(d.MaHoaDon) &&
					d.MaHoaDon.StartsWith("HD"))
				.ToList();


			// ============================================================
			// LƯU THỐNG KÊ
			//
			// Tổng quy đổi dùng để xác định thuốc bán chạy nhất.
			// Chi tiết ĐVT dùng để hiển thị:
			//
			// 2 Hộp, 3 Vỉ, 5 Viên
			// ============================================================
			Dictionary<string, int> tongQuyDoi =
				new Dictionary<string, int>(
					StringComparer.OrdinalIgnoreCase
				);

			Dictionary<string, Dictionary<string, int>> chiTietDonVi =
				new Dictionary<string, Dictionary<string, int>>(
					StringComparer.OrdinalIgnoreCase
				);


			foreach (var don in danhSachDonTrongThang)
			{
				if (string.IsNullOrWhiteSpace(don.ChiTietDonHang))
					continue;


				string chiTiet = don.ChiTietDonHang.Trim();


				// ========================================================
				// FORMAT MỚI
				//
				// Paracetamol 500mg [Hộp] x1;
				// Paracetamol 500mg [Vỉ] x2;
				// Berberin 100mg [Viên] x5
				// ========================================================
				if (chiTiet.Contains(" [") ||
					chiTiet.Contains(";"))
				{
					string[] cacMon =
						chiTiet.Split(
							new[] { ';' },
							StringSplitOptions.RemoveEmptyEntries
						);


					foreach (string monRaw in cacMon)
					{
						string mon = monRaw.Trim();

						if (string.IsNullOrWhiteSpace(mon))
							continue;


						// --------------------------------------------
						// LẤY SỐ LƯỢNG
						// --------------------------------------------
						int soLuong = 1;

						int viTriX =
							mon.LastIndexOf(
								" x",
								StringComparison.OrdinalIgnoreCase
							);


						string phanTenVaDonVi = mon;


						if (viTriX >= 0)
						{
							string chuoiSoLuong =
								mon.Substring(
									viTriX + 2
								).Trim();


							int.TryParse(
								chuoiSoLuong,
								out soLuong
							);


							if (soLuong <= 0)
								soLuong = 1;


							phanTenVaDonVi =
								mon.Substring(
									0,
									viTriX
								).Trim();
						}


						// --------------------------------------------
						// LẤY ĐVT
						// --------------------------------------------
						string donVi = "";
						string tenThuoc = phanTenVaDonVi;

						int viTriMo =
							phanTenVaDonVi.LastIndexOf('[');

						int viTriDong =
							phanTenVaDonVi.LastIndexOf(']');


						if (viTriMo >= 0 &&
							viTriDong > viTriMo)
						{
							donVi =
								phanTenVaDonVi.Substring(
									viTriMo + 1,
									viTriDong - viTriMo - 1
								).Trim();


							tenThuoc =
								phanTenVaDonVi.Substring(
									0,
									viTriMo
								).Trim();
						}


						if (string.IsNullOrWhiteSpace(tenThuoc))
							continue;


						// --------------------------------------------
						// TÌM THUỐC GỐC
						// --------------------------------------------
						var thuoc =
							QuanLyNhaThuocData.DanhSachThuoc
							.FirstOrDefault(t =>
								t != null &&
								string.Equals(
									t.TenThuoc,
									tenThuoc,
									StringComparison.OrdinalIgnoreCase
								)
							);


						// --------------------------------------------
						// QUY ĐỔI RA ĐƠN VỊ NHỎ NHẤT
						//
						// Dùng để so sánh công bằng:
						//
						// 1 Hộp không thể coi ngang 1 Viên.
						// --------------------------------------------
						int soLuongQuyDoi = soLuong;


						if (thuoc != null)
						{
							int soVi =
								thuoc.SoViTrongHop > 0
								? thuoc.SoViTrongHop
								: 1;

							int soVien =
								thuoc.SoVienTrongVi > 0
								? thuoc.SoVienTrongVi
								: 1;


							if (donVi.Equals(
								"Viên",
								StringComparison.OrdinalIgnoreCase))
							{
								soLuongQuyDoi =
									soLuong;
							}
							else if (donVi.Equals(
								"Vỉ",
								StringComparison.OrdinalIgnoreCase))
							{
								soLuongQuyDoi =
									soLuong * soVien;
							}
							else
							{
								// Hộp / Lọ / Tuýp / đơn vị chính
								soLuongQuyDoi =
									soLuong *
									soVi *
									soVien;
							}
						}


						// --------------------------------------------
						// CỘNG TỔNG QUY ĐỔI
						// --------------------------------------------
						if (!tongQuyDoi.ContainsKey(tenThuoc))
						{
							tongQuyDoi[tenThuoc] = 0;
						}


						tongQuyDoi[tenThuoc] +=
							soLuongQuyDoi;


						// --------------------------------------------
						// CỘNG THEO ĐVT ĐỂ HIỂN THỊ
						// --------------------------------------------
						if (!chiTietDonVi.ContainsKey(tenThuoc))
						{
							chiTietDonVi[tenThuoc] =
								new Dictionary<string, int>(
									StringComparer.OrdinalIgnoreCase
								);
						}


						string donViHienThi =
							string.IsNullOrWhiteSpace(donVi)
							? "SP"
							: donVi;


						if (!chiTietDonVi[tenThuoc]
							.ContainsKey(donViHienThi))
						{
							chiTietDonVi[tenThuoc][donViHienThi] = 0;
						}


						chiTietDonVi[tenThuoc][donViHienThi] +=
							soLuong;
					}
				}


				// ========================================================
				// FORMAT CŨ
				//
				// Paracetamol 500mg:1,Panadol Extra:2
				//
				// Giữ lại để lịch sử cũ vẫn được thống kê.
				// ========================================================
				else
				{
					string[] cacMon =
						chiTiet.Split(
							new[] { ',' },
							StringSplitOptions.RemoveEmptyEntries
						);


					foreach (string monRaw in cacMon)
					{
						string mon =
							monRaw.Trim();


						int viTriHaiCham =
							mon.LastIndexOf(':');


						if (viTriHaiCham <= 0)
							continue;


						string tenThuoc =
							mon.Substring(
								0,
								viTriHaiCham
							).Trim();


						string chuoiSL =
							mon.Substring(
								viTriHaiCham + 1
							).Trim();


						if (!int.TryParse(
								chuoiSL,
								out int soLuong))
						{
							continue;
						}


						var thuoc =
							QuanLyNhaThuocData.DanhSachThuoc
							.FirstOrDefault(t =>
								t != null &&
								string.Equals(
									t.TenThuoc,
									tenThuoc,
									StringComparison.OrdinalIgnoreCase
								)
							);


						int quyDoi = soLuong;


						if (thuoc != null)
						{
							int soVi =
								thuoc.SoViTrongHop > 0
								? thuoc.SoViTrongHop
								: 1;

							int soVien =
								thuoc.SoVienTrongVi > 0
								? thuoc.SoVienTrongVi
								: 1;


							quyDoi =
								soLuong *
								soVi *
								soVien;
						}


						if (!tongQuyDoi.ContainsKey(tenThuoc))
						{
							tongQuyDoi[tenThuoc] = 0;
						}


						tongQuyDoi[tenThuoc] +=
							quyDoi;


						if (!chiTietDonVi.ContainsKey(tenThuoc))
						{
							chiTietDonVi[tenThuoc] =
								new Dictionary<string, int>();
						}


						if (!chiTietDonVi[tenThuoc]
							.ContainsKey("SP"))
						{
							chiTietDonVi[tenThuoc]["SP"] = 0;
						}


						chiTietDonVi[tenThuoc]["SP"] +=
							soLuong;
					}
				}
			}


			// ============================================================
			// KHÔNG CÓ ĐƠN TRONG THÁNG
			// ============================================================
			if (tongQuyDoi.Count == 0)
			{
				lblTopTenThuoc.Text =
					"Chưa có dữ liệu";

				lblTopChiTiet.Text =
					"Hãy bán đơn hàng đầu tiên";

				return;
			}


			// ============================================================
			// LẤY TOP 1
			// ============================================================
			var top1 =
				tongQuyDoi
				.OrderByDescending(x => x.Value)
				.First();


			string tenTop =
				top1.Key;


			lblTopTenThuoc.Text =
				tenTop;


			// ============================================================
			// HIỂN THỊ ĐÚNG ĐVT ĐÃ BÁN
			//
			// VD:
			// Đã bán: 2 Hộp, 3 Vỉ, 5 Viên
			// ============================================================
			if (chiTietDonVi.ContainsKey(tenTop))
			{
				string chiTietBan =
					string.Join(
						", ",
						chiTietDonVi[tenTop]
						.Where(x => x.Value > 0)
						.Select(x =>
							$"{x.Value:N0} {x.Key}")
					);


				lblTopChiTiet.Text =
					"Đã bán: " + chiTietBan;
			}
			else
			{
				lblTopChiTiet.Text =
					"Đã có giao dịch trong tháng";
			}
		}
		// =========================================================================
		// QUẢN LÝ KHO THUỐC & KPI CẢNH BÁO TỒN KHO
		// =========================================================================
		private void BuildKhoThuocView()
		{
			mainContentPanel.Controls.Clear();

			// 1. THANH THỐNG KÊ KPI (Chiều cao 85px gọn gàng)
			TableLayoutPanel pnlKPI = new TableLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = 85,
				ColumnCount = 3,
				RowCount = 1,
				Padding = new Padding(0, 0, 0, 10)
			};
			pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			pnlKPI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

			// Kiểm tra ngưỡng tồn kho (Cảnh báo khi dưới 25 Hộp)
			int nguongHopCanhBao = 25;
			int soThuocSapHet = QuanLyNhaThuocData.DanhSachThuoc.Count(t =>
			{
				int quyDoiHop = (t.SoViTrongHop * t.SoVienTrongVi) > 0 ? (t.SoViTrongHop * t.SoVienTrongVi) : 1;
				return (t.SoLuongTonVien / quyDoiHop) < nguongHopCanhBao;
			});

			decimal tongGiaTriKho = QuanLyNhaThuocData.DanhSachThuoc.Sum(t =>
			{
				int quyDoiHop = (t.SoViTrongHop * t.SoVienTrongVi) > 0 ? (t.SoViTrongHop * t.SoVienTrongVi) : 1;
				return (t.SoLuongTonVien / (decimal)quyDoiHop) * t.GiaBan;
			});

			// Thẻ 1: Top bán chạy
			// Khai báo biến hứng

			// Truyền 7 tham số (gọi hàm mới)
			// ĐỪNG khai báo "Label lblTopTenThuoc;" ở đây nữa nhé!

			// Xóa chữ Label ở phần "out", dùng luôn biến toàn cục đã tạo ở trên
			Panel cardTop = CreateKPICard("🔥 TOP BÁN CHẠY THÁNG NÀY", "Đang tải...", "Đang tải...",
				Color.FromArgb(230, 244, 234),
				Color.FromArgb(20, 108, 46),
				out lblTopTenThuoc,
				out lblTopChiTiet);
			CapNhatTopThuocBanChay();
			string subTextHet = soThuocSapHet > 0 ? "👉 Mặc hàng < 25 sản phẩm" : "Tồn kho an toàn";
			Panel cardHet = CreateKPICard("⚠️ BÁO ĐỘNG TỒN KHO", $"{soThuocSapHet} Mặt hàng sắp hết", subTextHet, Color.FromArgb(254, 237, 232), Color.FromArgb(192, 57, 43));

			cardHet.Cursor = Cursors.Hand;
			cardHet.DoubleClick += (s, e) => LocThuocSapHet(nguongHopCanhBao);
			foreach (Control c in cardHet.Controls)
			{
				c.Cursor = Cursors.Hand;
				c.DoubleClick += (s, e) => LocThuocSapHet(nguongHopCanhBao);
			}

			// Thẻ 3: Tổng giá trị vốn kho
			Panel cardTong = CreateKPICard("💰 TỔNG GIÁ TRỊ KHO", $"{tongGiaTriKho:N0} VNĐ", "Tính theo giá bán niêm yết hiện tại", Color.FromArgb(232, 240, 254), Color.FromArgb(26, 115, 232));

			pnlKPI.Controls.Add(cardTop, 0, 0);
			pnlKPI.Controls.Add(cardHet, 1, 0);
			pnlKPI.Controls.Add(cardTong, 2, 0);

			// 2. GROUPBOX DANH SÁCH THUỐC
			GroupBox gb = new GroupBox { Text = "📋 DANH SÁCH THUỐC TRONG KHO", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(12) };

			Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 45 };
			Label lblSearch = new Label { Text = "Tìm kiếm:", Location = new Point(0, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };

			txtKhoSearch = new TextBox { Location = new Point(75, 7), Width = 350, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
			txtKhoSearch.TextChanged += (s, e) => LoadKhoThuocGrid(txtKhoSearch.Text);

			Button btnNhapHang = new Button { Text = "📦 Nhập Hàng Tồn", Location = new Point(440, 5), Width = 140, Height = 32, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
			btnNhapHang.Click += BtnNhapHang_Click;

			Button btnXoaSearch = new Button { Text = "❌ Xóa Tìm Kiếm", Location = new Point(590, 5), Width = 130, Height = 32, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
			btnXoaSearch.FlatAppearance.BorderSize = 0;
			btnXoaSearch.Click += (s, e) => {
				txtKhoSearch.Clear();
				LoadKhoThuocGrid("");
			};

			pnlTop.Controls.AddRange(new Control[] { lblSearch, txtKhoSearch, btnNhapHang, btnXoaSearch });

			dgvKhoThuoc = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvKhoThuoc);

			dgvKhoThuoc.CellDoubleClick += (s, e) => {
				if (e.RowIndex >= 0 && dgvKhoThuoc.Rows[e.RowIndex].Cells["Tên_Thuốc"].Value != null)
				{
					txtKhoSearch.Text = dgvKhoThuoc.Rows[e.RowIndex].Cells["Tên_Thuốc"].Value.ToString();
				}
			};

			gb.Controls.AddRange(new Control[] { dgvKhoThuoc, pnlTop });

			mainContentPanel.Controls.Add(gb);
			mainContentPanel.Controls.Add(pnlKPI);
			pnlKPI.SendToBack();

			LoadKhoThuocGrid("");
		}


		// 🎯 HÀM 1: TÔ MÀU ĐỎ NHẠT DÒNG CÓ THUỐC SẮP HẾT

		// 🎯 HÀM LỌC DANH SÁCH THUỐC SẮP HẾT & MÀU NỀN ĐỎ NHẠT DỊU MẮT
		private void LocThuocSapHet(int nguongHop)
		{
			txtKhoSearch.Clear();
			var dsSapHet = QuanLyNhaThuocData.DanhSachThuoc
				.Where(t =>
				{
					int quyDoiHop = (t.SoViTrongHop * t.SoVienTrongVi) > 0 ? (t.SoViTrongHop * t.SoVienTrongVi) : 1;
					return (t.SoLuongTonVien / quyDoiHop) < nguongHop;
				})
				.Select(t => new
				{
					Mã = t.MaThuoc,
					Tên_Thuốc = t.TenThuoc,
					Thành_Phần = t.ThanhPhan,
					Cơ_Sở_Sản_Xuất = t.CoSoSanXuat,
					ĐVT = t.DonViTinh,
					Giá_Bán = t.GiaBan.ToString("N0") + " VNĐ",
					Số_Lượng_Tồn = t.TonKhoHienThi + " (⚠️ CẦN NHẬP)"
				}).ToList();

			dgvKhoThuoc.DataSource = dsSapHet;

			// Tô màu đỏ pastel nhạt (Đồng bộ chuẩn màu thẻ KPI)
			foreach (DataGridViewRow row in dgvKhoThuoc.Rows)
			{
				row.DefaultCellStyle.BackColor = Color.FromArgb(254, 237, 232);
				row.DefaultCellStyle.ForeColor = Color.FromArgb(192, 57, 43);
				row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 205, 195);
				row.DefaultCellStyle.SelectionForeColor = Color.DarkRed;
			}
		}
		// 🎯 HÀM TẠO THẺ KPI TỐI ƯU KHOẢNG CÁCH CHỮ (ĐÃ THÊM BIẾN OUT)
		private Panel CreateKPICard(string title, string mainValue, string subValue, Color bgColor, Color textColor, out Label outLblVal, out Label outLblSub)
		{
			Panel card = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = bgColor,
				Margin = new Padding(4),
				Padding = new Padding(10, 6, 10, 6)
			};

			Label lblTitle = new Label { Text = title, Location = new Point(8, 6), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = textColor };
			Label lblVal = new Label { Text = mainValue, Location = new Point(8, 25), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = textColor };
			Label lblSub = new Label { Text = subValue, Location = new Point(8, 50), AutoSize = true, Font = new Font("Segoe UI", 8.2F, FontStyle.Italic), ForeColor = Color.DimGray };

			// Gán 2 cái Label này ra biến ngoài để code tính toán có thể chèn text mới vào
			outLblVal = lblVal;
			outLblSub = lblSub;

			card.Controls.AddRange(new Control[] { lblTitle, lblVal, lblSub });
			return card;
		}
		// HÀM CŨ (5 THAM SỐ) - ĐỂ CỨU CÁC LỖI CS7036 BẠN ĐANG GẶP
		private Panel CreateKPICard(string title, string mainValue, string subValue, Color bgColor, Color textColor)
		{
			// C# sẽ tự động đẩy sang hàm 7 tham số, dùng 'out _' để vứt bỏ 2 biến không cần thiết
			return CreateKPICard(title, mainValue, subValue, bgColor, textColor, out _, out _);
		}
		private void LoadKhoThuocGrid(string tuKhoa = "")
		{
			var query = QuanLyNhaThuocData.DanhSachThuoc.AsEnumerable();

			// 🎯 LỌC DỮ LIỆU THEO TÊN, MÃ, THÀNH PHẦN HOẶC NHÀ SẢN XUẤT
			if (!string.IsNullOrWhiteSpace(tuKhoa))
			{
				string tk = tuKhoa.Trim().ToLower();
				query = query.Where(t => (t.TenThuoc != null && t.TenThuoc.ToLower().Contains(tk))
									  || (t.MaThuoc != null && t.MaThuoc.ToLower().Contains(tk))
									  || (t.ThanhPhan != null && t.ThanhPhan.ToLower().Contains(tk))
									  || (t.CoSoSanXuat != null && t.CoSoSanXuat.ToLower().Contains(tk)));
			}

			var ds = query.Select(t => new
			{
				Mã = t.MaThuoc,
				Tên_Thuốc = t.TenThuoc,
				Thành_Phần = t.ThanhPhan,
				Cơ_Sở_Sản_Xuất = t.CoSoSanXuat,
				ĐVT = t.DonViTinh,
				Giá_Bán = t.GiaBan.ToString("N0") + " VNĐ",
				Số_Lượng_Tồn = t.TonKhoHienThi
			}).ToList();

			dgvKhoThuoc.DataSource = ds;
		}

		// 🎯 2. SỬA SỰ KIỆN TÌM KIẾM THEO REALTIME
		// Trong hàm BuildKhoThuocView(), đảm bảo gán sự kiện tìm kiếm như sau:
		// txtKhoSearch.TextChanged += (s, e) => LoadKhoThuocGrid(txtKhoSearch.Text);

		// 🎯 3. BẮT BUỘC CHỌN THUỐC TRƯỚC KHI MỞ POPUP NHẬP HÀNG
		private void BtnNhapHang_Click(object sender, EventArgs e)
		{
			if (dgvKhoThuoc.CurrentRow == null || dgvKhoThuoc.CurrentRow.Cells["Mã"].Value == null)
			{
				MessageBox.Show("Vui lòng chọn 1 loại thuốc trong danh sách trước khi bấm Nhập Hàng!",
								"Chưa chọn thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string maThuoc = dgvKhoThuoc.CurrentRow.Cells["Mã"].Value.ToString();
			var thuocChon = QuanLyNhaThuocData.DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == maThuoc);

			if (thuocChon != null)
			{
				using (FormNhapKho frm = new FormNhapKho(thuocChon))
				{
					if (frm.ShowDialog() == DialogResult.OK)
					{
						// Giữ nguyên từ khóa tìm kiếm hiện tại sau khi nhập kho xong
						LoadKhoThuocGrid(txtKhoSearch.Text);
					}
				}
			}
		}
		// =========================================================================
		// KHÁCH HÀNG VIP
		// =========================================================================
		private void BuildKhachHangView()
		{
			mainContentPanel.Controls.Clear();

			TableLayoutPanel pnlMainLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 2
			};
			pnlMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105F));
			pnlMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			// KHU VỰC 4 THẺ THỐNG KÊ (Sử dụng hệ màu Pastel)
			TableLayoutPanel pnlCards = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 4,
				RowCount = 1,
				Margin = new Padding(0, 0, 0, 5)
			};
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

			Label l1v, l1s, l2v, l2s, l3v, l3s, l4v, l4s;
			// Card 1: Vàng Kem Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(254, 247, 224), Color.FromArgb(180, 100, 0), "🏆 TOP 1 ĐIỂM VIP", out l1v, out l1s), 0, 0);
			// Card 2: Tím Nhạt Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(243, 232, 255), Color.FromArgb(107, 33, 168), "👑 TOP SPENDER", out l2v, out l2s), 1, 0);
			// Card 3: Xanh Lá Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(230, 244, 234), Color.FromArgb(20, 108, 46), "👥 TỔNG SỐ KHÁCH VIP", out l3v, out l3s), 2, 0);
			// Card 4: Xanh Dương Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(232, 240, 254), Color.FromArgb(26, 115, 232), "🌟 TỔNG ĐIỂM HỆ THỐNG", out l4v, out l4s), 3, 0);

			var topDiem = QuanLyNhaThuocData.DanhSachKhachHang.OrderByDescending(k => k.DiemKhaDung).FirstOrDefault();
			if (topDiem != null) { l1v.Text = topDiem.HoTen; l1s.Text = $"Tích lũy: {topDiem.DiemKhaDung:N0} điểm"; }

			var topSpender = QuanLyNhaThuocData.DanhSachDonHang.Where(d => d.SoDienThoai != "KHACH-LE")
				.GroupBy(d => d.SoDienThoai).Select(g => new { Sdt = g.Key, Total = g.Sum(x => x.TongTien) })
				.OrderByDescending(x => x.Total).FirstOrDefault();
			if (topSpender != null)
			{
				var kh = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == topSpender.Sdt);
				l2v.Text = kh != null ? kh.HoTen : topSpender.Sdt;
				l2s.Text = $"Tổng chi: {topSpender.Total:N0} VNĐ";
			}

			l3v.Text = QuanLyNhaThuocData.DanhSachKhachHang.Count.ToString() + " Khách";
			l3s.Text = "Đã đăng ký thẻ thành viên";

			long totalDiem = QuanLyNhaThuocData.DanhSachKhachHang.Sum(k => (long)k.DiemKhaDung);
			l4v.Text = totalDiem.ToString("N0") + " Điểm";
			l4s.Text = $"Tương đương quy đổi {(totalDiem * 10):N0} VNĐ";

			pnlMainLayout.Controls.Add(pnlCards, 0, 0);

			// LAYOUT NỘI DUNG DƯỚI
			TableLayoutPanel pnlBottomLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 1,
				Margin = new Padding(0)
			};
			pnlBottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
			pnlBottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

			// CỘT 1: DANH SÁCH KHÁCH HÀNG
			GroupBox gb = new GroupBox { Text = "👥 DANH SÁCH KHÁCH HÀNG VIP", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(8) };

			Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 45 };
			Label lblSearch = new Label { Text = "Nhập SĐT / Tên:", Location = new Point(0, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
			txtKhachSearch = new TextBox { Location = new Point(120, 7), Width = 220, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };
			txtKhachSearch.TextChanged += (s, e) => LoadKhachHangGrid();

			Button btnXoaTimKiem = new Button { Text = "🧹 Xóa", Location = new Point(350, 5), Width = 80, Height = 32, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
			btnXoaTimKiem.Click += (s, e) => {
				txtKhachSearch.Text = "";
				txtKhachSearch.Focus();
			};

			pnlTop.Controls.AddRange(new Control[] { lblSearch, txtKhachSearch, btnXoaTimKiem });

			dgvKhachHang = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvKhachHang);

			dgvKhachHang.CellDoubleClick += (s, e) => {
				if (e.RowIndex >= 0) OpenCustomerDetails();
			};

			gb.Controls.AddRange(new Control[] { dgvKhachHang, pnlTop });

			// CỘT 2: TOP 10 KHÁCH VIP
			GroupBox gbTop10 = new GroupBox { Text = "🏆 BẢNG XẾP HẠNG TOP 10 KHÁCH VIP", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(8) };

			DataGridView dgvTop10Vip = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvTop10Vip);

			var dsTop10 = QuanLyNhaThuocData.DanhSachKhachHang
				.OrderByDescending(k => k.DiemKhaDung)
				.Take(10)
				.Select((k, index) => new
				{
					Hạng = $"Top {index + 1}",
					Họ_Tên = k.HoTen,
					SĐT = k.SoDienThoai,
					Điểm_VIP = k.DiemKhaDung,
					Trị_Giá = (k.DiemKhaDung * 10).ToString("N0") + "đ"
				}).ToList();

			dgvTop10Vip.DataSource = dsTop10;
			gbTop10.Controls.Add(dgvTop10Vip);

			pnlBottomLayout.Controls.Add(gb, 0, 0);
			pnlBottomLayout.Controls.Add(gbTop10, 1, 0);

			pnlMainLayout.Controls.Add(pnlBottomLayout, 0, 1);

			mainContentPanel.Controls.Add(pnlMainLayout);

			LoadKhachHangGrid();
		}

		private Panel CreateStatCard(Color bg, string title, out Label lblVal, out Label lblSub)
		{
			Panel card = new Panel { Dock = DockStyle.Fill, BackColor = bg, Margin = new Padding(4), Padding = new Padding(12, 8, 12, 8) };

			Label lblT = new Label { Text = title, Dock = DockStyle.Top, Height = 22, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			lblVal = new Label { Text = "--", Dock = DockStyle.Top, Height = 32, ForeColor = Color.White, Font = new Font("Segoe UI", 13F, FontStyle.Bold) };
			lblSub = new Label { Text = "--", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(240, 240, 240), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic) };

			card.Controls.AddRange(new Control[] { lblSub, lblVal, lblT });
			return card;
		}

		private void OpenCustomerDetails()
		{
			if (dgvKhachHang.CurrentRow == null) return;
			string sdt = dgvKhachHang.CurrentRow.Cells["SĐT"].Value.ToString();
			var kh = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == sdt);
			if (kh != null)
			{
				FormChiTietKhachHangInline frm = new FormChiTietKhachHangInline(kh);
				if (frm.ShowDialog() == DialogResult.OK)
				{
					LoadKhachHangGrid();
				}
			}
		}

		private void LoadKhachHangGrid()
		{
			var dsKhach = QuanLyNhaThuocData.DanhSachKhachHang;
			if (dsKhach == null) return;

			// Khi mở màn Khách Hàng VIP cũng kiểm tra hạn bảo lưu,
			// để trường hợp đổi ngày hệ thống khi app đang mở vẫn cập nhật đúng.
			CapNhatLaiToanBoHangVip();

			string kw = txtKhachSearch?.Text?.Trim().ToLower() ?? "";

			// 🎯 NẠP BẢNG CHÍNH DGV_KHACHHANG
			dgvKhachHang.DataSource = null;
			dgvKhachHang.DataSource = dsKhach
				.Where(k => k != null && (string.IsNullOrEmpty(kw) ||
							(k.SoDienThoai != null && k.SoDienThoai.Contains(kw)) ||
							(k.HoTen != null && k.HoTen.ToLower().Contains(kw))))
				.Select(k => new {
					SĐT = k.SoDienThoai ?? "",
					Họ_Tên = k.HoTen ?? "",
					// Hiển thị hạng thực sự đang được bảo lưu
					Hạng_VIP = k.HangVip,
					Điểm_VIP = k.DiemKhaDung,
					Trị_Giá_Đổi = $"{k.DiemKhaDung * 10:N0} VNĐ"
				}).ToList();

			// 🎯 ÉP NỐI SỰ KIỆN TÔ MÀU BẢNG (Gỡ ra gán lại để không trùng sự kiện)
			dgvKhachHang.CellFormatting -= dgvKhachHang_CellFormatting;
			dgvKhachHang.CellFormatting += dgvKhachHang_CellFormatting;
		}

		// =========================================================================
		// ĐỔI QUÀ
		// =========================================================================
		private ComboBox cboLocKhoangDiem;
		private DataGridView dgvQuaTangView;
		private DataGridView dgvKhachHangDoiQua;
		private TextBox txtSearchKhachDoiQua;
		private KhachHang selectedKhachHangDoiQua = null;
		private Label lblVipHint = new Label();


		// TÍCH CHUỘT LÊN DÒNG KHÁCH HÀNG -> ĐIỀN SĐT VÀO Ô TÌM KIẾM
		private void dgvKhachHangDoiQua_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || dgvKhachHangDoiQua.CurrentRow == null) return;

			string sdt = dgvKhachHangDoiQua.CurrentRow.Cells["SĐT"].Value?.ToString();

			if (!string.IsNullOrEmpty(sdt))
			{
				// 🎯 Tự động điền SĐT lên ô tìm kiếm
				isUpdatingSearchText = true;
				txtSearchKhachDoiQua.Text = sdt;
				isUpdatingSearchText = false;

				// 🎯 CHỈ MỞ POPUP KHI CLICK ĐÚNG VÀO CỘT "Cảnh_Báo_Điểm"
				if (dgvKhachHangDoiQua.Columns[e.ColumnIndex].Name == "Cảnh_Báo_Điểm")
				{
					var kh = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == sdt);
					if (kh != null && kh.ThongBaoHetHan == "⚠️")
					{
						HienThiPopupChiTietDiem(kh);
					}
				}
			}
		}

		// LỌC DANH SÁCH KHI GÕ VÀO Ô TÌM KIẾM
		private void txtSearchKhachDoiQua_TextChanged(object sender, EventArgs e)
		{
			if (isUpdatingSearchText) return;
			LoadKhachHangDoiQuaGrid(txtSearchKhachDoiQua.Text.Trim());
		}

		// BẤM NÚT XÓA -> TẢI LẠI TOÀN BỘ DANH SÁCH
		private void btnXoaTimKiemKhachHang_Click(object sender, EventArgs e)
		{
			isUpdatingSearchText = true;
			txtSearchKhachDoiQua.Clear();
			isUpdatingSearchText = false;

			LoadKhachHangDoiQuaGrid("");
			txtSearchKhachDoiQua.Focus();
		}
		// =========================================================================
		// 1. MÀN HÌNH ĐỔI QUÀ VIP (CÓ NÚT XÓA + CLICK BẢNG TỰ ĐIỀN SĐT)
		// =========================================================================
		// Khai báo ComboBox lọc quà ở cấp Class Form1 để các hàm khác gọi lại được
		private ComboBox cboLocDiemQua;

		public void BuildDoiQuaView()
		{
			mainContentPanel.Controls.Clear();

			TableLayoutPanel pnlMain = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 1
			};
			pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

			// =========================================================================
			// CỘT 1: DANH SÁCH QUÀ TẶNG (CÓ ICON 📌 VÀ IN ĐẬM LỌC KHOẢNG ĐIỂM)
			// =========================================================================
			GroupBox gbQua = new GroupBox
			{
				Text = "🎁 1. DANH SÁCH QUÀ TẶNG THIẾT YẾU",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				Padding = new Padding(8, 25, 8, 8)
			};

			Panel pnlFilterQua = new Panel { Dock = DockStyle.Top, Height = 40 };

			// 🎯 THÊM LẠI ICON 📌 VÀ IN ĐẬM TẠI ĐÂY
			Label lblFilter = new Label
			{
				Text = "📌 Lọc khoảng điểm:",
				Location = new Point(8, 9),
				AutoSize = true,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
			};

			// Chỉnh tọa độ X = 155 để tạo khoảng trống thoáng cho dòng chữ đậm + icon
			cboLocDiemQua = new ComboBox
			{
				Location = new Point(155, 6),
				Width = 210,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
			};

			cboLocDiemQua.Items.Clear();
			cboLocDiemQua.Items.AddRange(new object[] {
		"-- Tất cả quà tặng --",
		"Dưới 1.000 điểm",
		"Từ 1.000 - 5.000 điểm",
		"Trên 5.000 điểm",
		"🎁 Quà đủ điểm đổi (theo khách chọn)"
	});
			cboLocDiemQua.SelectedIndex = 0;

			pnlFilterQua.Controls.Add(lblFilter);
			pnlFilterQua.Controls.Add(cboLocDiemQua);

			dgvQuaTangView = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvQuaTangView);

			gbQua.Controls.Add(dgvQuaTangView);
			gbQua.Controls.Add(pnlFilterQua);
			pnlFilterQua.SendToBack();
			dgvQuaTangView.BringToFront();

			// =========================================================================
			// CỘT 2: TÌM KHÁCH HÀNG & NÚT XÓA
			// =========================================================================
			GroupBox gbKhach = new GroupBox
			{
				Text = "👥 2. TÌM KHÁCH HÀNG  CẢNH BÁO HẠN ĐIỂM",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				Padding = new Padding(8, 25, 8, 8)
			};

			Panel pnlSearchGroup = new Panel { Dock = DockStyle.Top, Height = 40 };

			Label lblSearch = new Label
			{
				Text = "SĐT/Tên Khách:",
				Location = new Point(8, 9),
				AutoSize = true,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
			};

			txtSearchKhachDoiQua = new TextBox
			{
				Location = new Point(125, 6),
				Width = 190,
				Font = new Font("Segoe UI", 10F)
			};

			Button btnXoaSearchKhach = new Button
			{
				Text = "✖ Xóa",
				Location = new Point(322, 5),
				Width = 65,
				Height = 28,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(220, 53, 69),
				ForeColor = Color.White,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold),
				Cursor = Cursors.Hand
			};

			pnlSearchGroup.Controls.Add(lblSearch);
			pnlSearchGroup.Controls.Add(txtSearchKhachDoiQua);
			pnlSearchGroup.Controls.Add(btnXoaSearchKhach);

			Button btnXacNhanDoiQua = new Button
			{
				Text = "🎁 XÁC NHẬN TRỪ ĐIỂM GIAO QUÀ",
				Dock = DockStyle.Bottom,
				Height = 45,
				BackColor = Color.FromArgb(230, 126, 34),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 11F, FontStyle.Bold),
				Cursor = Cursors.Hand
			};

			dgvKhachHangDoiQua = new DataGridView { Dock = DockStyle.Fill };
			dgvKhachHangDoiQua.CellDoubleClick += dgvKhachHangDoiQua_CellDoubleClick;
			StyleDataGridView(dgvKhachHangDoiQua);

			gbKhach.Controls.Add(dgvKhachHangDoiQua);
			gbKhach.Controls.Add(pnlSearchGroup);
			gbKhach.Controls.Add(btnXacNhanDoiQua);

			pnlSearchGroup.SendToBack();
			dgvKhachHangDoiQua.BringToFront();

			pnlMain.Controls.Add(gbQua, 0, 0);
			pnlMain.Controls.Add(gbKhach, 1, 0);

			mainContentPanel.Controls.Add(pnlMain);

			// =========================================================================
			// GÁN SỰ KIỆN XỬ LÝ (SỬ DỤNG LAMBDA ĐỂ TRÁNH LỖI MISSING METHOD)
			// =========================================================================
			cboLocDiemQua.SelectedIndexChanged += (s, e) => ApplyQuaTangFilter();

			txtSearchKhachDoiQua.TextChanged -= txtSearchKhachDoiQua_TextChanged;
			txtSearchKhachDoiQua.TextChanged += txtSearchKhachDoiQua_TextChanged;

			btnXoaSearchKhach.Click -= btnXoaTimKiemKhachHang_Click;
			btnXoaSearchKhach.Click += btnXoaTimKiemKhachHang_Click;

			dgvKhachHangDoiQua.CellClick -= dgvKhachHangDoiQua_CellClick;
			dgvKhachHangDoiQua.CellClick += dgvKhachHangDoiQua_CellClick;

			btnXacNhanDoiQua.Click -= BtnXacNhanDoiQua_Click;
			btnXacNhanDoiQua.Click += BtnXacNhanDoiQua_Click;

			LoadGridQuaTang(QuanLyQuaTangData.DanhSachQua);
			LoadKhachHangDoiQuaGrid("");
		}
		private void CboLocDiemQua_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyQuaTangFilter();
		}
		// 🎯 HÀM LỌC DANH SÁCH QUÀ TẶNG (GỒM CẢ TÍNH NĂNG LỌC THEO ĐIỂM KHÁCH CHỌN)
		private void ApplyQuaTangFilter()
		{
			if (cboLocDiemQua == null || QuanLyQuaTangData.DanhSachQua == null) return;

			var dsQua = QuanLyQuaTangData.DanhSachQua.AsEnumerable();

			switch (cboLocDiemQua.SelectedIndex)
			{
				case 1:
					dsQua = dsQua.Where(q => q.DiemCan < 1000);
					break;
				case 2:
					dsQua = dsQua.Where(q => q.DiemCan >= 1000 && q.DiemCan <= 5000);
					break;
				case 3:
					dsQua = dsQua.Where(q => q.DiemCan > 5000);
					break;
				case 4: // 🎁 Quà đủ điểm đổi (theo khách chọn)
					string sdt = txtSearchKhachDoiQua.Text.Trim();
					var khach = QuanLyNhaThuocData.DanhSachKhachHang?.FirstOrDefault(k => k.SoDienThoai == sdt);

					if (khach != null)
					{
						dsQua = dsQua.Where(q => q.DiemCan <= khach.DiemKhaDung);
					}
					else
					{
						MessageBox.Show("Vui lòng tích chọn một khách hàng ở bảng bên phải trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						cboLocDiemQua.SelectedIndex = 0;
						return;
					}
					break;
			}

			LoadGridQuaTang(dsQua.ToList());
		}


		// =========================================================================
		// 2. MÀN HÌNH QUẢN LÝ KHÁCH HÀNG VIP
		// =========================================================================

		private void UpdateSelectedKhachHang()
		{
			if (dgvKhachHangDoiQua.CurrentRow != null)
			{
				string sdt = dgvKhachHangDoiQua.CurrentRow.Cells["SĐT"].Value?.ToString();
				selectedKhachHangDoiQua = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == sdt);
				if (cboLocKhoangDiem != null && cboLocKhoangDiem.SelectedIndex == 4) ApplyFilterQuaTang();
			}
			else
			{
				selectedKhachHangDoiQua = null;
			}
		}

		private void ApplyFilterQuaTang()
		{
			int index = cboLocKhoangDiem.SelectedIndex;
			List<QuaTang> filtered;

			switch (index)
			{
				case 1:
					filtered = QuanLyQuaTangData.DanhSachQua.Where(q => q.DiemCan < 1500).ToList();
					break;
				case 2:
					filtered = QuanLyQuaTangData.DanhSachQua.Where(q => q.DiemCan >= 1500 && q.DiemCan <= 5000).ToList();
					break;
				case 3:
					filtered = QuanLyQuaTangData.DanhSachQua.Where(q => q.DiemCan > 5000).ToList();
					break;
				case 4:
					int diemKhach = selectedKhachHangDoiQua != null ? selectedKhachHangDoiQua.DiemKhaDung : 0;
					filtered = QuanLyQuaTangData.DanhSachQua.Where(q => q.DiemCan <= diemKhach).ToList();
					break;
				default:
					filtered = QuanLyQuaTangData.DanhSachQua;
					break;
			}

			LoadGridQuaTang(filtered);
		}

		private void LoadGridQuaTang(List<QuaTang> list)
		{
			if (dgvQuaTangView == null) return;
			dgvQuaTangView.DataSource = list.Select(q => new
			{
				Mã_Quà = q.MaQua,
				Tên_Sản_Phẩm = q.TenSanPham,
				Điểm_Cần = q.DiemCan.ToString("N0") + " điểm",
				Giá_Thực = q.TriGia.ToString("N0") + " VNĐ",
				Tồn_Kho = q.SoLuongTon
			}).ToList();
		}

		// 4. HÀM LOAD BẢNG & LỌC THEO TỪ KHÓA
		private void LoadKhachHangDoiQuaGrid(string keyword = "", string selectedSdt = "")
		{
			if (dgvKhachHangDoiQua == null || QuanLyNhaThuocData.DanhSachKhachHang == null) return;

			string kw = keyword?.Trim().ToLower() ?? "";
			var ds = QuanLyNhaThuocData.DanhSachKhachHang;

			if (!string.IsNullOrEmpty(kw))
			{
				ds = ds.Where(k => k != null &&
					((k.SoDienThoai != null && k.SoDienThoai.Contains(kw)) ||
					 (k.HoTen != null && k.HoTen.ToLower().Contains(kw)))).ToList();
			}

			// 🎯 1. TẮT TẠM SỰ KIỆN ĐỂ KHÔNG BỊ BÁO LỖI NULL KHI CLEAR BẢNG
			dgvKhachHangDoiQua.SelectionChanged -= DgvKhachHangPos_SelectionChanged;

			dgvKhachHangDoiQua.DataSource = null;
			dgvKhachHangDoiQua.DataSource = ds.Select(k => new
			{
				SĐT = k.SoDienThoai ?? "",
				Họ_Tên = k.HoTen ?? "",
				Điểm_Khả_Dụng = $"{k.DiemKhaDung:N0} điểm",
				Cảnh_Báo_Điểm = k.ThongBaoHetHan,
				Đã_Đổi_Hôm_Nay = $"{k.SoLanDoiHomNay}/3 lần"
			}).ToList();

			// 🎯 2. MỞ LẠI SỰ KIỆN SAU KHI ĐÃ NẠP XONG DỮ LIỆU
			dgvKhachHangDoiQua.SelectionChanged += DgvKhachHangPos_SelectionChanged;

			// 🎯 3. GIỮ CON TRỎ Ở ĐÚNG KHÁCH HÀNG VỪA ĐỔI QUÀ
			if (!string.IsNullOrEmpty(selectedSdt) && dgvKhachHangDoiQua.Rows.Count > 0)
			{
				foreach (DataGridViewRow row in dgvKhachHangDoiQua.Rows)
				{
					if (row.Cells["SĐT"].Value?.ToString() == selectedSdt)
					{
						row.Selected = true;
						dgvKhachHangDoiQua.CurrentCell = row.Cells[0];
						break;
					}
				}
			}
		}
		private bool TruDiemUuTienHetHan(KhachHang kh, int diemCanTru)
		{
			if (kh.DiemKhaDung < diemCanTru) return false;

			var listBatches = kh.DanhSachDiem
				.Where(b => b.NgayHetHan.Date >= DateTime.Now.Date)
				.OrderBy(b => b.NgayHetHan)
				.ToList();

			int diemConThieu = diemCanTru;
			foreach (var batch in listBatches)
			{
				if (batch.SoDiem <= diemConThieu)
				{
					diemConThieu -= batch.SoDiem;
					batch.SoDiem = 0;
				}
				else
				{
					batch.SoDiem -= diemConThieu;
					diemConThieu = 0;
					break;
				}
			}

			kh.DanhSachDiem.RemoveAll(b => b.SoDiem <= 0);
			return true;
		}

		private bool KiemTraGioiHanDoiTrongNgay(KhachHang kh, int gioiHanLan = 3)
		{
			if (kh.NgayDoiGanNhat.Date != DateTime.Now.Date)
			{
				kh.SoLanDoiHomNay = 0;
				kh.NgayDoiGanNhat = DateTime.Now.Date;
			}

			return kh.SoLanDoiHomNay < gioiHanLan;
		}

		// 5. Nút ĐỔI QUÀ (Đổi xong giữ nguyên con trỏ tại khách hàng đó)
		// 5. Nút ĐỔI QUÀ (Đổi xong giữ nguyên con trỏ tại khách hàng đó)
		private void BtnXacNhanDoiQua_Click(object sender, EventArgs e)
		{
			if (dgvKhachHangDoiQua.CurrentRow == null || dgvQuaTangView.CurrentRow == null)
			{
				MessageBox.Show("Vui lòng chọn Khách hàng và Quà tặng muốn đổi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string sdt = dgvKhachHangDoiQua.CurrentRow.Cells["SĐT"].Value?.ToString();
			var kh = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == sdt);

			string maQua = dgvQuaTangView.CurrentRow.Cells["Mã_Quà"].Value?.ToString();
			var qua = QuanLyQuaTangData.DanhSachQua.FirstOrDefault(q => q.MaQua == maQua);

			if (kh != null && qua != null)
			{
				// 1. Kiểm tra giới hạn đổi quà trong ngày (Tối đa 3 lần)
				if (!KiemTraGioiHanDoiTrongNgay(kh, 3))
				{
					MessageBox.Show("Khách hàng này đã vượt quá giới hạn 3 lần đổi quà trong ngày!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// 2. Kiểm tra đủ điểm khả dụng
				if (kh.DiemKhaDung < qua.DiemCan)
				{
					MessageBox.Show($"Khách hàng không đủ điểm! Cần {qua.DiemCan:N0} điểm nhưng chỉ có {kh.DiemKhaDung:N0} điểm.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// 3. Trừ điểm FIFO, tăng lượt đổi trong ngày và giảm tồn kho quà
				kh.TruDiem(qua.DiemCan);
				kh.SoLanDoiHomNay++;
				kh.NgayDoiGanNhat = DateTime.Now;
				qua.SoLuongTon--;

				// 4. Ghi nhận lịch sử đổi quà chi tiết
				var donDoiQua = new LichSuMuaHang
				{
					MaHoaDon = "DQ" + DateTime.Now.ToString("yyMMddHHmmss"),
					NgayMua = DateTime.Now,
					SoDienThoai = kh.SoDienThoai,
					MaHangHoa = qua.MaQua,
					DiemCong = 0,
					DiemTru = qua.DiemCan,
					TongTien = 0,
					GhiChu = $"Đổi quà: {qua.TenSanPham}"
				};

				if (kh.DanhSachLichSu == null) kh.DanhSachLichSu = new List<LichSuMuaHang>();
				kh.DanhSachLichSu.Add(donDoiQua);

				// Lưu lịch sử đơn hàng chung nếu hệ thống dùng
				if (QuanLyNhaThuocData.DanhSachDonHang != null)
				{
					QuanLyNhaThuocData.DanhSachDonHang.Add(donDoiQua);
					QuanLyNhaThuocData.LuuFileDonHang();
				}

				// 5. Lưu điểm số đã trừ vĩnh viễn xuống file KhachHang.txt
				QuanLyNhaThuocData.LuuFileKhachHang();

				MessageBox.Show($"Đổi thành công quà [{qua.TenSanPham}] cho khách {kh.HoTen}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

				// 6. Nạp lại dữ liệu và GIỮ NGUYÊN CON TRỎ ở đúng khách hàng đang chọn
				LoadKhachHangDoiQuaGrid(txtSearchKhachDoiQua.Text.Trim(), sdt);
				LoadGridQuaTang(QuanLyQuaTangData.DanhSachQua);
			}
		}
		private void DgvKhachHang_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			var dgv = sender as DataGridView;
			if (dgv == null || dgv.Rows[e.RowIndex] == null) return;

			var row = dgv.Rows[e.RowIndex];
			string timKiem = "";

			if (dgv.Columns.Contains("SĐT") && row.Cells["SĐT"].Value != null)
			{
				timKiem = row.Cells["SĐT"].Value.ToString();
			}
			else if (dgv.Columns.Contains("Họ_Tên") && row.Cells["Họ_Tên"].Value != null)
			{
				timKiem = row.Cells["Họ_Tên"].Value.ToString();
			}
			else
			{
				timKiem = row.Cells[0].Value?.ToString() ?? "";
			}

			var kh = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k =>
				k.SoDienThoai == timKiem || k.HoTen == timKiem);

			if (kh != null)
			{
				CapNhatGoiYVip(kh);
				HienThiPopupChiTietDiem(kh);
			}
			else
			{
				MessageBox.Show("Không tìm thấy thông tin lô điểm của khách hàng này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void HienThiPopupChiTietDiem(KhachHang kh)
		{
			Form fPopup = new Form
			{
				Text = $"Chi tiết Lô Điểm Sắp Hết Hạn - Khách hàng: {kh.HoTen}",
				Width = 620,
				Height = 380,
				StartPosition = FormStartPosition.CenterParent,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false
			};

			Label lblHeader = new Label
			{
				Text = $"👤 Khách hàng: {kh.HoTen} ({kh.SoDienThoai})\n💎 TỔNG ĐIỂM KHẢ DỤNG: {kh.DiemKhaDung:N0} điểm",
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				ForeColor = Color.DarkBlue,
				Dock = DockStyle.Top,
				Height = 55,
				Padding = new Padding(10, 8, 0, 0)
			};

			DataGridView dgvBatches = new DataGridView
			{
				Dock = DockStyle.Fill,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				ReadOnly = true,
				AllowUserToAddRows = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect
			};

			// 🎯 CHỈ LẤY CÁC LÔ ĐIỂM CÓ HẠN SỬ DỤNG <= 30 NGÀY (LÔ CHƯA TỚI HẠN 1 THÁNG SẼ BỊ ẨN)
			var dsHienThi = kh.DanhSachDiem
				.Where(b => b.SoDiem > 0 && (b.NgayHetHan.Date - DateTime.Now.Date).Days <= 30)
				.OrderBy(b => b.NgayHetHan)
				.Select(b => {
					int soNgay = (b.NgayHetHan.Date - DateTime.Now.Date).Days;
					return new
					{
						Số_Điểm = $"{b.SoDiem:N0} điểm",
						Ngày_Giờ_Hết_Hạn = b.NgayHetHan.ToString("dd/MM/yyyy HH:mm"),
						Còn_Lại = soNgay < 0 ? "Đã hết hạn" : $"{soNgay} ngày",
						Trạng_Thái = $"⚠️ Sắp hết hạn (còn {soNgay} ngày)"
					};
				}).ToList();

			dgvBatches.DataSource = dsHienThi;

			fPopup.Controls.Add(dgvBatches);
			fPopup.Controls.Add(lblHeader);
			fPopup.ShowDialog();
		}
		private void HienThiPopupLichSuDoiQua(KhachHang kh)
		{
			Form frm = new Form
			{
				Text = $"🎁 LỊCH SỬ ĐỔI QUÀ TẶNG - KH: {kh.HoTen.ToUpper()} ({kh.SoDienThoai})",
				Width = 720,
				Height = 400,
				StartPosition = FormStartPosition.CenterParent,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false
			};

			DataGridView dgvLichSu = new DataGridView
			{
				Dock = DockStyle.Fill,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				ReadOnly = true,
				AllowUserToAddRows = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				BackgroundColor = Color.White
			};

			// 🎯 LỌC LINH HOẠT: Lấy giao dịch có trừ điểm VÀ (Mã bắt đầu bằng DQ HOẶC Ghi chú có chữ "Đổi quà" / tên quà)
			var dsDoiQua = kh.DanhSachLichSu
	.Where(l => l.MaHoaDon.StartsWith("DQ")) // 🎯 Chỉ lấy giao dịch Đổi Quà
	.Select(l => new
	{
		Mã_Đổi_Quà = l.MaHoaDon,
		Thời_Gian = l.NgayMua.ToString("dd/MM/yyyy HH:mm"),
		Quà_Tặng = l.GhiChu.Replace("Đổi quà: ", ""), // Chỉ hiển thị tên quà
		Điểm_Đã_Dùng = $"-{l.DiemTru:N0} điểm"
	}).OrderByDescending(l => l.Thời_Gian).ToList();

			if (!dsDoiQua.Any())
			{
				MessageBox.Show($"Khách hàng {kh.HoTen} chưa từng thực hiện đổi quà tặng nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			dgvLichSu.DataSource = dsDoiQua;

			dgvLichSu.CellFormatting += (s, e) =>
			{
				if (e.RowIndex >= 0 && dgvLichSu.Columns[e.ColumnIndex].Name == "Điểm_Đã_Dùng")
				{
					e.CellStyle.ForeColor = Color.DarkRed;
					e.CellStyle.Font = new Font(dgvLichSu.Font, FontStyle.Bold);
				}
			};

			frm.Controls.Add(dgvLichSu);
			frm.ShowDialog(this);
		}
		private void dgvKhachHangDoiQua_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// Bỏ qua tiêu đề cột, chỉ bắt khi đúp vào dòng dữ liệu
			if (e.RowIndex < 0 || dgvKhachHangDoiQua.CurrentRow == null) return;

			string sdt = dgvKhachHangDoiQua.Rows[e.RowIndex].Cells["SĐT"].Value?.ToString();
			if (string.IsNullOrEmpty(sdt)) return;

			var khach = QuanLyNhaThuocData.DanhSachKhachHang.FirstOrDefault(k => k.SoDienThoai == sdt);
			if (khach != null)
			{
				HienThiPopupLichSuDoiQua(khach); // Mở đúng popup Lịch Sử Đổi Quà
			}
		}
		// =========================================================================
		// TRUNG TÂM BÁO CÁO & THỐNG KÊ (DASHBOARD)
		// =========================================================================
		private void BuildBaoCaoView()
		{
			mainContentPanel.Controls.Clear();

			TableLayoutPanel pnlMainLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 3,
				Padding = new Padding(6)
			};
			pnlMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));
			pnlMainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			pnlMainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			// 1. HEADER CARD (Hệ màu Pastel dịu mắt)
			TableLayoutPanel pnlCards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

			// Card 1: Xanh Lá Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(230, 244, 234), Color.FromArgb(20, 108, 46), "💵 DOANH THU THÁNG", out lblBaoCaoDoanhThuVal, out lblBaoCaoDoanhThuSub), 0, 0);
			// Card 2: Xanh Dương Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(232, 240, 254), Color.FromArgb(26, 115, 232), "👥 TỔNG ĐƠN HÀNG", out lblBaoCaoTongDonVal, out lblBaoCaoTongDonSub), 1, 0);
			// Card 3: Tím Nhạt Pastel
			pnlCards.Controls.Add(CreateStatCard(Color.FromArgb(243, 232, 255), Color.FromArgb(107, 33, 168), "🏆 DOANH THU CẢ NĂM", out lblBaoCaoDoanhThuNamVal, out lblBaoCaoDoanhThuNamSub), 2, 0);

			// 2. FILTER BAR
			Panel pnlFilter = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 243, 246), Margin = new Padding(4, 2, 4, 4) };

			Label lblFilter = new Label { Text = "📅 Chọn Mốc Báo Cáo:", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };

			Label lblThang = new Label { Text = "Tháng:", Location = new Point(180, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
			cboBaoCaoThang = new ComboBox { Location = new Point(230, 6), Width = 95, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
			for (int i = 1; i <= 12; i++) cboBaoCaoThang.Items.Add("Tháng " + i);
			cboBaoCaoThang.SelectedIndex = DateTime.Now.Month - 1;

			Label lblNam = new Label { Text = "Năm:", Location = new Point(345, 10), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
			numBaoCaoNam = new NumericUpDown { Location = new Point(388, 6), Width = 85, Minimum = 2020, Maximum = 2035, Value = DateTime.Now.Year, Font = new Font("Segoe UI", 9.5F) };

			cboBaoCaoThang.SelectedIndexChanged += (s, e) => LoadDataBaoCao();
			numBaoCaoNam.ValueChanged += (s, e) => LoadDataBaoCao();

			pnlFilter.Controls.AddRange(new Control[] { lblFilter, lblThang, cboBaoCaoThang, lblNam, numBaoCaoNam });

			// 3. NỘI DUNG DƯỚI
			TableLayoutPanel pnlContent = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
			pnlContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
			pnlContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

			GroupBox gbHistory = new GroupBox { Text = "📜 LỊCH SỬ GIAO DỊCH TRONG THÁNG", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
			dgvBaoCaoLichSu = new DataGridView { Dock = DockStyle.Fill };
			StyleDataGridView(dgvBaoCaoLichSu);
			gbHistory.Controls.Add(dgvBaoCaoLichSu);

			GroupBox gbChart = new GroupBox { Text = "📈 BIỂU ĐỒ DOANH THU 12 THÁNG", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
			pnlChartDoanhThu = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
			pnlChartDoanhThu.Paint += PnlChartDoanhThu_Paint;
			pnlChartDoanhThu.Resize += (s, e) => pnlChartDoanhThu.Invalidate();
			gbChart.Controls.Add(pnlChartDoanhThu);

			pnlContent.Controls.Add(gbHistory, 0, 0);
			pnlContent.Controls.Add(gbChart, 1, 0);

			pnlMainLayout.Controls.Add(pnlCards, 0, 0);
			pnlMainLayout.Controls.Add(pnlFilter, 0, 1);
			pnlMainLayout.Controls.Add(pnlContent, 0, 2);

			mainContentPanel.Controls.Add(pnlMainLayout);

			LoadDataBaoCao();
		}

		// =========================================================================
		// 3. HÀM TẠO THẺ STAT CARD (CĂN CHỈNH KHOẢNG CÁCH CHỮ VÀ NỀN SÁNG)
		// =========================================================================
		private Panel CreateStatCard(Color bgColor, Color textColor, string title, out Label valLabel, out Label subLabel)
		{
			Panel card = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = bgColor,
				Margin = new Padding(4),
				Padding = new Padding(8, 6, 8, 6)
			};

			Label lblTitle = new Label { Text = title, Location = new Point(8, 6), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = textColor };
			valLabel = new Label { Text = "---", Location = new Point(8, 25), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = textColor };
			subLabel = new Label { Text = "---", Location = new Point(8, 50), AutoSize = true, Font = new Font("Segoe UI", 8.2F, FontStyle.Italic), ForeColor = Color.DimGray };

			card.Controls.AddRange(new Control[] { lblTitle, valLabel, subLabel });
			return card;
		}

		private void LoadDataBaoCao()
		{
			if (cboBaoCaoThang == null || numBaoCaoNam == null) return;

			int nam = (int)numBaoCaoNam.Value;
			int thang = cboBaoCaoThang.SelectedIndex + 1;

			var dsThang = QuanLyNhaThuocData.DanhSachDonHang
				.Where(d => d.NgayMua.Year == nam && d.NgayMua.Month == thang)
				.OrderByDescending(d => d.NgayMua)
				.ToList();

			decimal doanhThuNam = QuanLyNhaThuocData.DanhSachDonHang
				.Where(d => d.NgayMua.Year == nam)
				.Sum(d => d.TongTien);

			decimal doanhThuThang = dsThang.Sum(d => d.TongTien);

			lblBaoCaoDoanhThuVal.Text = $"{doanhThuThang:N0} VNĐ";
			lblBaoCaoDoanhThuSub.Text = $"Tổng thu tháng {thang}/{nam}";

			lblBaoCaoTongDonVal.Text = $"{dsThang.Count} Đơn hàng";
			lblBaoCaoTongDonSub.Text = "Bao gồm cả khách lẻ, VIP";

			lblBaoCaoDoanhThuNamVal.Text = $"{doanhThuNam:N0} VNĐ";
			lblBaoCaoDoanhThuNamSub.Text = $"Tổng tích lũy cả năm {nam}";

			dgvBaoCaoLichSu.DataSource = dsThang.Select(d => new
			{
				Mã_HD = d.MaHoaDon,
				Thời_Gian = d.NgayMua.ToString("dd/MM HH:mm"),
				SĐT_Khách = d.SoDienThoai,
				Tổng_Thu = d.TongTien.ToString("N0") + "đ",
				Điểm_Trừ = d.DiemTru
			}).ToList();

			pnlChartDoanhThu.Invalidate();
		}

		private void PnlChartDoanhThu_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			int nam = (int)numBaoCaoNam.Value;
			int thangChon = cboBaoCaoThang.SelectedIndex + 1;

			decimal[] doanhThu12Thang = new decimal[12];
			for (int i = 1; i <= 12; i++)
			{
				doanhThu12Thang[i - 1] = QuanLyNhaThuocData.DanhSachDonHang
					.Where(d => d.NgayMua.Year == nam && d.NgayMua.Month == i)
					.Sum(d => d.TongTien);
			}

			decimal maxVal = doanhThu12Thang.Max();
			if (maxVal == 0) maxVal = 1;

			int width = pnlChartDoanhThu.Width;
			int height = pnlChartDoanhThu.Height;
			int paddingLeft = 30;
			int paddingBottom = 30;
			int paddingTop = 25;
			int paddingRight = 10;

			int chartWidth = width - paddingLeft - paddingRight;
			int chartHeight = height - paddingTop - paddingBottom;

			using (Pen gridPen = new Pen(Color.FromArgb(235, 238, 242), 1))
			{
				for (int i = 0; i <= 4; i++)
				{
					int y = paddingTop + (chartHeight / 4) * i;
					g.DrawLine(gridPen, paddingLeft, y, width - paddingRight, y);
				}
			}

			int barWidth = (chartWidth / 12) - 6;
			if (barWidth < 4) barWidth = 4;

			using (Font fontLabel = new Font("Segoe UI", 8F, FontStyle.Bold))
			using (Font fontVal = new Font("Segoe UI", 7.5F, FontStyle.Regular))
			using (Brush textBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
			using (Brush activeBrush = new SolidBrush(Color.FromArgb(255, 128, 0)))
			using (Brush normalBrush = new SolidBrush(Color.FromArgb(0, 122, 204)))
			{
				for (int i = 0; i < 12; i++)
				{
					int x = paddingLeft + i * (chartWidth / 12) + 3;
					int barH = (int)((doanhThu12Thang[i] / maxVal) * (chartHeight - 10));
					int y = paddingTop + (chartHeight - barH);

					Brush currentBarBrush = (i + 1 == thangChon) ? activeBrush : normalBrush;

					if (barH > 0)
					{
						g.FillRectangle(currentBarBrush, x, y, barWidth, barH);
					}

					g.DrawString($"T{i + 1}", fontLabel, textBrush, x + (barWidth / 2) - 9, height - paddingBottom + 5);

					if (doanhThu12Thang[i] > 0)
					{
						string valText = doanhThu12Thang[i] >= 1000000
							? $"{doanhThu12Thang[i] / 1000000:0.#}M"
							: $"{doanhThu12Thang[i] / 1000:0}k";

						g.DrawString(valText, fontVal, textBrush, x - 2, Math.Max(5, y - 16));
					}
				}
			}
		}

		// =========================================================================
		// FORM PHỤ: NHẬP HÀNG
		// =========================================================================
		public class FormNhapHangInline : Form
		{
			public FormNhapHangInline(Thuoc thuoc)
			{
				this.Text = "Nhập Thêm Tồn Kho - " + thuoc.TenThuoc;
				this.Size = new Size(380, 200);
				this.StartPosition = FormStartPosition.CenterParent;

				Label lblInfo = new Label { Text = $"Thuốc: {thuoc.TenThuoc}\nTồn hiện tại: {thuoc.SoLuongTon}", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
				Label lblNum = new Label { Text = "Số lượng nhập thêm:", Location = new Point(20, 75), AutoSize = true };
				NumericUpDown numAdd = new NumericUpDown { Location = new Point(170, 72), Width = 150, Minimum = 1, Maximum = 10000, Value = 10 };

				Button btnOk = new Button { Text = "Xác Nhận Nhập", Location = new Point(170, 110), Width = 150, Height = 35, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
				btnOk.Click += (s, e) => { thuoc.SoLuongTon += (int)numAdd.Value; QuanLyNhaThuocData.LuuFileThuoc(); this.DialogResult = DialogResult.OK; this.Close(); };

				this.Controls.AddRange(new Control[] { lblInfo, lblNum, numAdd, btnOk });
			}
		}
	}

	// =========================================================================
	// FORM PHỤ: CHI TIẾT KHÁCH HÀNG
	// =========================================================================
	public class FormChiTietKhachHangInline : Form
	{
		private KhachHang currentKhachHang;
		private TextBox txtHoTen, txtSdt;
		private ComboBox cboThang;
		private NumericUpDown numNam;
		private DateTimePicker dtpTuNgay, dtpDenNgay;
		private DataGridView dgvLichSu;

		public FormChiTietKhachHangInline(KhachHang kh)
		{
			this.currentKhachHang = kh;
			this.Text = "CHI TIẾT KHÁCH HÀNG VIP - " + kh.HoTen;
			this.Size = new Size(930, 600);
			this.StartPosition = FormStartPosition.CenterParent;
			this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
			this.BackColor = Color.FromArgb(245, 247, 250);

			GroupBox gbInfo = new GroupBox
			{
				Text = "⚙️ THÔNG TIN CÁ NHÂN & TÍCH ĐIỂM VIP",
				Dock = DockStyle.Top,
				Height = 125,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				Padding = new Padding(10)
			};

			Label lblSdt = new Label { Text = "SĐT Khách:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			txtSdt = new TextBox { Text = kh.SoDienThoai, Location = new Point(100, 27), Width = 150, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };

			Label lblHoTen = new Label { Text = "Họ và Tên:", Location = new Point(270, 30), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			txtHoTen = new TextBox { Text = kh.HoTen, Location = new Point(345, 27), Width = 200, Font = new Font("Segoe UI", 9.5F, FontStyle.Regular) };

			Label lblDiem = new Label
			{
				Text = $"⭐ Điểm VIP: {kh.DiemKhaDung:N0} điểm (= {(kh.DiemKhaDung * 10):N0} VNĐ)",
				Location = new Point(15, 68),
				AutoSize = true,
				Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
				ForeColor = Color.DarkGreen
			};

			Button btnLuu = new Button { Text = "💾 Lưu Thay Đổi", Location = new Point(590, 25), Width = 135, Height = 32, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnLuu.Click += BtnLuu_Click;

			Button btnXoa = new Button { Text = "🗑️ Xóa Khách Hàng", Location = new Point(740, 25), Width = 135, Height = 32, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnXoa.Click += BtnXoa_Click;

			gbInfo.Controls.AddRange(new Control[] { lblSdt, txtSdt, lblHoTen, txtHoTen, lblDiem, btnLuu, btnXoa });

			GroupBox gbLichSu = new GroupBox
			{
				Text = "📜 LỊCH SỬ MUA HÀNG & BỘ LỌC THỜI GIAN",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				Padding = new Padding(10)
			};

			Panel pnlFilter = new Panel { Dock = DockStyle.Top, Height = 48 };

			Label lblLocThang = new Label { Text = "📅 Chọn Tháng:", Location = new Point(5, 12), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(24, 43, 73) };
			cboThang = new ComboBox { Location = new Point(105, 8), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			cboThang.Items.Add("-- Tùy chọn --");
			for (int i = 1; i <= 12; i++) cboThang.Items.Add("Tháng " + i);
			cboThang.SelectedIndex = 0;

			numNam = new NumericUpDown { Location = new Point(220, 8), Width = 70, Minimum = 2020, Maximum = 2035, Value = DateTime.Today.Year, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };

			cboThang.SelectedIndexChanged += (s, e) => ApplyMonthFilter();
			numNam.ValueChanged += (s, e) => { if (cboThang.SelectedIndex > 0) ApplyMonthFilter(); };

			Label lblTu = new Label { Text = "Từ ngày:", Location = new Point(310, 12), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			dtpTuNgay = new DateTimePicker { Location = new Point(368, 8), Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			dtpTuNgay.Value = DateTime.Today.AddMonths(-3);

			Label lblDen = new Label { Text = "Đến:", Location = new Point(485, 12), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			dtpDenNgay = new DateTimePicker { Location = new Point(522, 8), Format = DateTimePickerFormat.Short, Width = 110, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
			dtpDenNgay.Value = DateTime.Today;

			Button btnLoc = new Button { Text = "🔍 Lọc", Location = new Point(640, 7), Width = 80, Height = 30, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnLoc.Click += (s, e) => LoadLichSuGrid();

			Button btnReset = new Button { Text = "🔄 Tất Cả", Location = new Point(728, 7), Width = 90, Height = 30, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
			btnReset.Click += (s, e) => {
				cboThang.SelectedIndex = 0;
				dtpTuNgay.Value = DateTime.Today.AddYears(-5);
				dtpDenNgay.Value = DateTime.Today;
				LoadLichSuGrid();
			};

			pnlFilter.Controls.AddRange(new Control[] { lblLocThang, cboThang, numNam, lblTu, dtpTuNgay, lblDen, dtpDenNgay, btnLoc, btnReset });

			dgvLichSu = new DataGridView { Dock = DockStyle.Fill };
			dgvLichSu.BorderStyle = BorderStyle.FixedSingle;
			dgvLichSu.BackgroundColor = Color.White;
			dgvLichSu.EnableHeadersVisualStyles = false;
			dgvLichSu.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 43, 73);
			dgvLichSu.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvLichSu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dgvLichSu.RowHeadersVisible = false;
			dgvLichSu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvLichSu.ReadOnly = true;
			dgvLichSu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvLichSu.CellDoubleClick += (s, e) =>
			{
				if (e.RowIndex >= 0 && dgvLichSu.Rows[e.RowIndex].Cells["Mã_HD"].Value != null)
				{
					string maHD = dgvLichSu.Rows[e.RowIndex].Cells["Mã_HD"].Value?.ToString();
					string tongTien = dgvLichSu.Rows[e.RowIndex].Cells["Tổng_Tiền"].Value?.ToString();
					string chiTietText = dgvLichSu.Rows[e.RowIndex].Cells["Chi_Tiết_Thuốc_Đã_Mua"].Value?.ToString();

					HienThiChiTietHoaDonModal(maHD, tongTien, chiTietText);
				}
			};
			gbLichSu.Controls.AddRange(new Control[] { dgvLichSu, pnlFilter });

			this.Controls.AddRange(new Control[] { gbLichSu, gbInfo });

			LoadLichSuGrid();
		}

		private void HienThiChiTietHoaDonModal(string maHD, string tongTien, string chiTietChuoi)
		{
			Form frmDetail = new Form
			{
				Text = $"🧾 CHI TIẾT HÓA ĐƠN - {maHD}",
				Size = new Size(550, 420),
				StartPosition = FormStartPosition.CenterParent,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false,
				BackColor = Color.White
			};

			Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(240, 244, 248), Padding = new Padding(10) };
			Label lblTitle = new Label
			{
				Text = $"MÃ HÓA ĐƠN: {maHD}\nTổng thanh toán: {tongTien}",
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
				ForeColor = Color.FromArgb(24, 43, 73)
			};
			pnlTop.Controls.Add(lblTitle);

			DataGridView dgvDetail = new DataGridView
			{
				Dock = DockStyle.Fill,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				BackgroundColor = Color.White,
				BorderStyle = BorderStyle.None,
				RowHeadersVisible = false
			};
			dgvDetail.EnableHeadersVisualStyles = false;
			dgvDetail.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 43, 73);
			dgvDetail.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvDetail.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dgvDetail.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 250);
			dgvDetail.DefaultCellStyle.SelectionForeColor = Color.Black;
			dgvDetail.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);

			DataTable dt = new DataTable();

			dt.Columns.Add("STT", typeof(int));
			dt.Columns.Add("Tên Thuốc / Mặt Hàng", typeof(string));
			dt.Columns.Add("ĐVT", typeof(string));
			dt.Columns.Add("Số Lượng", typeof(string));


			if (!string.IsNullOrWhiteSpace(chiTietChuoi))
			{
				/*
				 * FORMAT MỚI:
				 *
				 * Paracetamol 500mg [Hộp] x1;
				 * Paracetamol 500mg [Vỉ] x1;
				 * Berocca Performance [Viên] x1
				 *
				 *
				 * FORMAT CŨ:
				 *
				 * Paracetamol 500mg:1,Panadol Extra:2
				 *
				 * Vẫn hỗ trợ để các hóa đơn cũ không bị lỗi.
				 */


				// =====================================================
				// 1. KIỂM TRA FORMAT MỚI
				// =====================================================
				if (chiTietChuoi.Contains(";") ||
					chiTietChuoi.Contains(" ["))
				{
					string[] items =
						chiTietChuoi.Split(
							new[] { ';' },
							StringSplitOptions.RemoveEmptyEntries
						);


					int stt = 1;


					foreach (string item in items)
					{
						string text = item.Trim();

						if (string.IsNullOrWhiteSpace(text))
							continue;


						string tenThuoc = text;
						string donViTinh = "";
						string soLuong = "1";


						// =================================================
						// LẤY SỐ LƯỢNG
						// Ví dụ:
						// Paracetamol [Hộp] x1
						// =================================================
						int viTriX =
							text.LastIndexOf(
								" x",
								StringComparison.OrdinalIgnoreCase
							);


						string phanThongTin =
							text;


						if (viTriX >= 0)
						{
							soLuong =
								text.Substring(
									viTriX + 2
								).Trim();


							phanThongTin =
								text.Substring(
									0,
									viTriX
								).Trim();
						}


						// =================================================
						// LẤY ĐƠN VỊ TÍNH
						//
						// Paracetamol 500mg [Vỉ]
						//                       ^
						// =================================================
						int viTriMoNgoac =
							phanThongTin.LastIndexOf('[');

						int viTriDongNgoac =
							phanThongTin.LastIndexOf(']');


						if (viTriMoNgoac >= 0 &&
							viTriDongNgoac > viTriMoNgoac)
						{
							donViTinh =
								phanThongTin.Substring(
									viTriMoNgoac + 1,
									viTriDongNgoac -
									viTriMoNgoac - 1
								).Trim();


							tenThuoc =
								phanThongTin.Substring(
									0,
									viTriMoNgoac
								).Trim();
						}
						else
						{
							tenThuoc =
								phanThongTin.Trim();
						}


						dt.Rows.Add(
							stt++,
							tenThuoc,
							donViTinh,
							soLuong
						);
					}
				}

				// =====================================================
				// 2. HỖ TRỢ HÓA ĐƠN CŨ
				//
				// Ví dụ:
				// Paracetamol 500mg:1,Panadol Extra:2
				// =====================================================
				else
				{
					string[] items =
						chiTietChuoi.Split(
							new[] { ',' },
							StringSplitOptions.RemoveEmptyEntries
						);


					int stt = 1;


					foreach (string item in items)
					{
						string text = item.Trim();

						if (string.IsNullOrWhiteSpace(text))
							continue;


						string tenThuoc = text;
						string soLuong = "1";


						int viTriHaiCham =
							text.LastIndexOf(':');


						if (viTriHaiCham > 0)
						{
							tenThuoc =
								text.Substring(
									0,
									viTriHaiCham
								).Trim();


							soLuong =
								text.Substring(
									viTriHaiCham + 1
								).Trim();
						}


						dt.Rows.Add(
							stt++,
							tenThuoc,
							"",
							soLuong
						);
					}
				}
			}
			dgvDetail.DataSource = dt;

			dgvDetail.DataSource = dt;

			Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(5) };
			Button btnClose = new Button
			{
				Text = "❌ Đóng",
				Dock = DockStyle.Right,
				Width = 100,
				BackColor = Color.FromArgb(108, 117, 125),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};
			btnClose.Click += (s, e) => frmDetail.Close();
			pnlBottom.Controls.Add(btnClose);

			frmDetail.Controls.Add(dgvDetail);
			frmDetail.Controls.Add(pnlTop);
			frmDetail.Controls.Add(pnlBottom);

			frmDetail.ShowDialog();
		}

		private void ApplyMonthFilter()
		{
			if (cboThang.SelectedIndex > 0)
			{
				int month = cboThang.SelectedIndex;
				int year = (int)numNam.Value;

				dtpTuNgay.Value = new DateTime(year, month, 1);
				dtpDenNgay.Value = new DateTime(year, month, DateTime.DaysInMonth(year, month));

				LoadLichSuGrid();
			}
		}

		private void BtnLuu_Click(object sender, EventArgs e)
		{
			string newSdt = txtSdt.Text.Trim();
			string newTen = txtHoTen.Text.Trim();

			if (string.IsNullOrEmpty(newSdt) || string.IsNullOrEmpty(newTen))
			{
				MessageBox.Show("SĐT và Họ Tên không được để trống!", "Lỗi");
				return;
			}

			if (newSdt != currentKhachHang.SoDienThoai && QuanLyNhaThuocData.DanhSachKhachHang.Any(k => k.SoDienThoai == newSdt))
			{
				MessageBox.Show("Số điện thoại này đã thuộc về khách hàng khác!", "Trùng SĐT");
				return;
			}

			string oldSdt = currentKhachHang.SoDienThoai;
			currentKhachHang.SoDienThoai = newSdt;
			currentKhachHang.HoTen = newTen;

			foreach (var hd in QuanLyNhaThuocData.DanhSachDonHang.Where(d => d.SoDienThoai == oldSdt))
			{
				hd.SoDienThoai = newSdt;
			}

			QuanLyNhaThuocData.LuuFileKhachHang();
			QuanLyNhaThuocData.LuuFileDonHang();

			MessageBox.Show("Cập nhật thông tin khách hàng thành công!", "Thông Báo");
			this.DialogResult = DialogResult.OK;
		}
		private void BtnXoa_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng [{currentKhachHang.HoTen}] khỏi hệ thống?", "Xác Nhận Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				QuanLyNhaThuocData.DanhSachKhachHang.Remove(currentKhachHang);
				QuanLyNhaThuocData.LuuFileKhachHang();

				MessageBox.Show("Đã xóa khách hàng!", "Thông Báo");
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}
		private void LoadLichSuGrid()
		{
			DateTime tuNgay = dtpTuNgay.Value.Date;
			DateTime denNgay = dtpDenNgay.Value.Date.AddDays(1).AddTicks(-1);

			var listFiltered = currentKhachHang.DanhSachLichSu
				.Where(d => d.NgayMua >= tuNgay && d.NgayMua <= denNgay)
				.OrderByDescending(d => d.NgayMua)
				.Select(d => new
				{
					Mã_HD = d.MaHoaDon,
					Thời_Gian = d.NgayMua.ToString("dd/MM/yyyy HH:mm"),
					Tổng_Tiền = d.TongTien.ToString("N0") + " VNĐ",
					Cộng_Điểm = "+" + d.DiemCong,
					Trừ_Điểm = "-" + d.DiemTru,
					Chi_Tiết_Thuốc_Đã_Mua = d.GhiChu
				}).ToList();

			dgvLichSu.DataSource = listFiltered;
		}
	}
}
