using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
	public partial class FormNhapKho : Form
	{
		private Thuoc _thuocChon;
		private Label lblTenThuoc;
		private Label lblHangSanXuat;
		private ComboBox cboNhaCungCap;
		private TextBox txtSoLo;
		private DateTimePicker dtpHanSuDung;
		private TextBox txtSoLuongNhap;
		private Label lblDonViNhap; // Đã chuyển từ ComboBox sang Label cố định
		private TextBox txtGiaNhap;
		private Label lblTongTien;
		private Button btnXacNhan;

		public FormNhapKho(Thuoc thuoc)
		{
			_thuocChon = thuoc;
			InitUI();
		}

		private void InitUI()
		{
			this.Text = "📦 PHIẾU NHẬP KHO THUỐC";
			this.Size = new Size(500, 440);
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;

			Panel pnlBottom = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 50,
				Padding = new Padding(10, 5, 15, 10)
			};

			btnXacNhan = new Button
			{
				Text = "✔ XÁC NHẬN NHẬP KHO",
				Dock = DockStyle.Right,
				Width = 190,
				Height = 35,
				BackColor = Color.FromArgb(40, 167, 69),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnXacNhan.FlatAppearance.BorderSize = 0;
			btnXacNhan.Click += BtnXacNhan_Click;
			pnlBottom.Controls.Add(btnXacNhan);

			TableLayoutPanel pnl = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(15, 10, 15, 0),
				RowCount = 8,
				ColumnCount = 2,
				Font = new Font("Segoe UI", 9.5F)
			};
			pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
			pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

			for (int i = 0; i < 8; i++)
			{
				pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
			}

			// 1. Mặt hàng
			pnl.Controls.Add(new Label { Text = "Mặt Hàng:", Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 0, 0);
			lblTenThuoc = new Label { Text = $"[{_thuocChon.MaThuoc}] {_thuocChon.TenThuoc}", Dock = DockStyle.Fill, ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
			pnl.Controls.Add(lblTenThuoc, 1, 0);

			// 2. Hãng sản xuất
			pnl.Controls.Add(new Label { Text = "Hãng Sản Xuất:", Anchor = AnchorStyles.Left }, 0, 1);
			lblHangSanXuat = new Label { Text = string.IsNullOrEmpty(_thuocChon.CoSoSanXuat) ? "Chưa cập nhật" : _thuocChon.CoSoSanXuat, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(30, 30, 30), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
			pnl.Controls.Add(lblHangSanXuat, 1, 1);

			// 3. Đại lý cấp hàng
			pnl.Controls.Add(new Label { Text = "Đại Lý Cấp Hàng:", Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 0, 2);
			cboNhaCungCap = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
			cboNhaCungCap.Items.AddRange(new object[] { "Công ty Cổ phần Codupha (Kho Sỉ)", "Công ty Dược phẩm CPC1 Hà Nội", "Tổng công ty Dược Việt Nam (Vinapharm)", "Công ty Phân phối Dược phẩm Zuellig Pharma" });
			cboNhaCungCap.SelectedIndex = 0;
			pnl.Controls.Add(cboNhaCungCap, 1, 2);

			// 4. Số lô
			pnl.Controls.Add(new Label { Text = "Số Lô (Batch No):", Anchor = AnchorStyles.Left }, 0, 3);
			txtSoLo = new TextBox { Dock = DockStyle.Fill, Text = "LOT-" + DateTime.Now.ToString("yyyyMMdd") };
			pnl.Controls.Add(txtSoLo, 1, 3);

			// 5. Hạn sử dụng
			pnl.Controls.Add(new Label { Text = "Hạn Sử Dụng (HSD):", Anchor = AnchorStyles.Left }, 0, 4);
			dtpHanSuDung = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Dock = DockStyle.Fill, Value = DateTime.Now.AddYears(2) };
			pnl.Controls.Add(dtpHanSuDung, 1, 4);

			// 6. Số lượng & ĐƠN VỊ TÍNH CỐ ĐỊNH (BỎ MŨI TÊN DROPDOWN)
			pnl.Controls.Add(new Label { Text = "Số Lượng Nhập:", Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 0, 5);
			FlowLayoutPanel pnlSL = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0) };
			txtSoLuongNhap = new TextBox { Width = 100, Text = "10" };

			// 🎯 Đổi ComboBox thành Label cố định (Hộp/Tuýp/Lọ...), tự động bắt đúng đơn vị nguyên gói
			string dvtChinh = string.IsNullOrWhiteSpace(_thuocChon.DonViTinh) ? "Hộp" : _thuocChon.DonViTinh;
			lblDonViNhap = new Label
			{
				Text = dvtChinh,
				AutoSize = true,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				ForeColor = Color.FromArgb(50, 50, 50),
				Margin = new Padding(10, 4, 0, 0)
			};

			pnlSL.Controls.Add(txtSoLuongNhap);
			pnlSL.Controls.Add(lblDonViNhap);
			pnl.Controls.Add(pnlSL, 1, 5);

			// 7. Đơn giá nhập
			pnl.Controls.Add(new Label { Text = "Đơn Giá Nhập (VNĐ):", Anchor = AnchorStyles.Left }, 0, 6);
			decimal giaVonGoiY = Math.Round(_thuocChon.GiaBan * 0.75m / 1000m) * 1000m;
			txtGiaNhap = new TextBox { Dock = DockStyle.Fill, Text = giaVonGoiY.ToString("0") };
			pnl.Controls.Add(txtGiaNhap, 1, 6);

			// 8. Thành tiền
			pnl.Controls.Add(new Label { Text = "Thành Tiền Lô:", Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }, 0, 7);
			lblTongTien = new Label { Dock = DockStyle.Fill, ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
			pnl.Controls.Add(lblTongTien, 1, 7);

			txtSoLuongNhap.TextChanged += (s, e) => TinhTongTien();
			txtGiaNhap.TextChanged += (s, e) => TinhTongTien();
			TinhTongTien();

			this.Controls.Add(pnl);
			this.Controls.Add(pnlBottom);
		}

		private void TinhTongTien()
		{
			if (int.TryParse(txtSoLuongNhap.Text, out int sl) && decimal.TryParse(txtGiaNhap.Text, out decimal gia))
			{
				lblTongTien.Text = (sl * gia).ToString("N0") + " VNĐ";
			}
			else
			{
				lblTongTien.Text = "0 VNĐ";
			}
		}

		private void BtnXacNhan_Click(object sender, EventArgs e)
		{
			if (!int.TryParse(txtSoLuongNhap.Text, out int slNhap) || slNhap <= 0)
			{
				MessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!decimal.TryParse(txtGiaNhap.Text, out decimal giaNhap) || giaNhap <= 0)
			{
				MessageBox.Show("Vui lòng nhập đơn giá hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string donViChon = lblDonViNhap.Text;

			// 🎯 TỰ ĐỘNG QUY ĐỔI SỐ VIÊN THEO ĐƠN VỊ TÍNH NGUYÊN ĐÓNG GÓI
			int soViTrongHop = _thuocChon.SoViTrongHop <= 0 ? 1 : _thuocChon.SoViTrongHop;
			int soVienTrongVi = _thuocChon.SoVienTrongVi <= 0 ? 1 : _thuocChon.SoVienTrongVi;

			int soVienQuyDoi = slNhap * soViTrongHop * soVienTrongVi;

			_thuocChon.SoLuongTonVien += soVienQuyDoi;
			QuanLyNhaThuocData.LuuFileThuoc();

			MessageBox.Show($"Đã nhập kho thành công cho [{_thuocChon.TenThuoc}]:\n• Số lượng: +{slNhap} {donViChon} ({soVienQuyDoi} viên)\n• Đơn giá: {giaNhap:N0} VNĐ/{donViChon}\n• Tổng tiền lô: {(slNhap * giaNhap):N0} VNĐ\n• HSD: {dtpHanSuDung.Value:dd/MM/yyyy}",
				"Nhập kho thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}