// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.EmployeesForm
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class EmployeesForm : Form
{
	private DatabaseHelper dbHelper;

	private DataTable dataTable;

	private DataTable shopsData;

	private IContainer components = null;

	private DataGridView dataGridViewEmployees;

	private Button btnUpdate;

	private Button btnDelete;

	private TextBox txtEmpFirstName;

	private TextBox txtEmpLastName;

	private TextBox txtEmpPatronymic;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private TextBox txtEmpPosition;

	private Label label5;

	private TextBox txtEmpPhone;

	private Label label6;

	private Button btnAdd;

	private ComboBox comboBoxShops;

	public EmployeesForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadShopsData();
		LoadData();
		SetupForm();
		btnAdd.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		dataGridViewEmployees.SelectionChanged += dataGridViewEmployees_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewEmployees.ReadOnly = true;
		dataGridViewEmployees.MultiSelect = false;
		dataGridViewEmployees.AllowUserToAddRows = false;
		dataGridViewEmployees.AllowUserToDeleteRows = false;
		SetupShopsComboBox();
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void SetupShopsComboBox()
	{
		DataTable comboData = shopsData.Copy();
		DataRow emptyRow = comboData.NewRow();
		emptyRow["shop_id"] = -1;
		emptyRow["name"] = "-- Не выбрано --";
		comboData.Rows.InsertAt(emptyRow, 0);
		comboBoxShops.DisplayMember = "name";
		comboBoxShops.ValueMember = "shop_id";
		comboBoxShops.DataSource = comboData;
	}

	private void LoadShopsData()
	{
		try
		{
			string query = "SELECT shop_id, name FROM Shops ORDER BY name";
			shopsData = dbHelper.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки списка магазинов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT e.*, s.name as shop_name \r\n                     FROM Employees e \r\n                     LEFT JOIN Shops s ON e.shop_id = s.shop_id \r\n                     ORDER BY e.employee_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewEmployees.DataSource = dataTable;
			if (dataGridViewEmployees.Columns.Contains("first_name"))
			{
				dataGridViewEmployees.Columns["first_name"].HeaderText = "Имя";
			}
			if (dataGridViewEmployees.Columns.Contains("last_name"))
			{
				dataGridViewEmployees.Columns["last_name"].HeaderText = "Фамилия";
			}
			if (dataGridViewEmployees.Columns.Contains("patronymic"))
			{
				dataGridViewEmployees.Columns["patronymic"].HeaderText = "Отчество";
			}
			if (dataGridViewEmployees.Columns.Contains("position"))
			{
				dataGridViewEmployees.Columns["position"].HeaderText = "Должность";
			}
			if (dataGridViewEmployees.Columns.Contains("phone_number"))
			{
				dataGridViewEmployees.Columns["phone_number"].HeaderText = "Телефон";
			}
			if (dataGridViewEmployees.Columns.Contains("shop_name"))
			{
				dataGridViewEmployees.Columns["shop_name"].HeaderText = "Магазин";
			}
			if (dataGridViewEmployees.Columns.Contains("employee_id"))
			{
				dataGridViewEmployees.Columns["employee_id"].Visible = false;
			}
			if (dataGridViewEmployees.Columns.Contains("shop_id"))
			{
				dataGridViewEmployees.Columns["shop_id"].Visible = false;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (ValidateRequiredFields())
			{
				object selectedShopId = comboBoxShops.SelectedValue;
				object shopIdValue = ((selectedShopId != null && selectedShopId != DBNull.Value && selectedShopId.ToString() != "-1") ? selectedShopId : DBNull.Value);
				string query = "INSERT INTO Employees (first_name, last_name, patronymic, shop_id, position, phone_number) \r\n                     VALUES (@firstName, @lastName, @patronymic, @shopId, @position, @phone)";
				NpgsqlParameter[] parameters = new NpgsqlParameter[6]
				{
					new NpgsqlParameter("@firstName", txtEmpFirstName.Text.Trim()),
					new NpgsqlParameter("@lastName", txtEmpLastName.Text.Trim()),
					new NpgsqlParameter("@patronymic", ((object)txtEmpPatronymic.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@shopId", shopIdValue),
					new NpgsqlParameter("@position", ((object)txtEmpPosition.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@phone", ((object)txtEmpPhone.Text.Trim()) ?? ((object)DBNull.Value))
				};
				int result = dbHelper.ExecuteNonQuery(query, parameters);
				if (result > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Сотрудник добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось добавить сотрудника!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении сотрудника: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewEmployees.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int empId = Convert.ToInt32(dataGridViewEmployees.CurrentRow.Cells["employee_id"].Value);
					object selectedShopId = comboBoxShops.SelectedValue;
					object shopIdValue = ((selectedShopId != null && selectedShopId != DBNull.Value && selectedShopId.ToString() != "-1") ? selectedShopId : DBNull.Value);
					string query = "UPDATE Employees SET \r\n                        first_name=@firstName, last_name=@lastName, patronymic=@patronymic, \r\n                        shop_id=@shopId, position=@position, phone_number=@phone \r\n                        WHERE employee_id=@empId";
					NpgsqlParameter[] parameters = new NpgsqlParameter[7]
					{
						new NpgsqlParameter("@firstName", txtEmpFirstName.Text.Trim()),
						new NpgsqlParameter("@lastName", txtEmpLastName.Text.Trim()),
						new NpgsqlParameter("@patronymic", ((object)txtEmpPatronymic.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@shopId", shopIdValue),
						new NpgsqlParameter("@position", ((object)txtEmpPosition.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@phone", ((object)txtEmpPhone.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@empId", empId)
					};
					int result = dbHelper.ExecuteNonQuery(query, parameters);
					if (result > 0)
					{
						LoadData();
						MessageBox.Show("Данные обновлены успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					else
					{
						MessageBox.Show("Не удалось обновить данные!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите сотрудника для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewEmployees.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранного сотрудника?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int empId = Convert.ToInt32(dataGridViewEmployees.CurrentRow.Cells["employee_id"].Value);
				string query = "DELETE FROM Employees WHERE employee_id=@empId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@empId", empId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Сотрудник удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить сотрудника!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении сотрудника: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите сотрудника для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void dataGridViewEmployees_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewEmployees.CurrentRow != null && dataGridViewEmployees.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (hasSelection)
		{
			DataGridViewRow row = dataGridViewEmployees.CurrentRow;
			txtEmpFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
			txtEmpLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";
			txtEmpPatronymic.Text = row.Cells["patronymic"].Value?.ToString() ?? "";
			txtEmpPosition.Text = row.Cells["position"].Value?.ToString() ?? "";
			txtEmpPhone.Text = row.Cells["phone_number"].Value?.ToString() ?? "";
			if (row.Cells["shop_id"].Value != null && row.Cells["shop_id"].Value != DBNull.Value)
			{
				int shopId = Convert.ToInt32(row.Cells["shop_id"].Value);
				comboBoxShops.SelectedValue = shopId;
			}
			else
			{
				comboBoxShops.SelectedValue = -1;
			}
		}
	}

	private void ClearFields()
	{
		txtEmpFirstName.Clear();
		txtEmpLastName.Clear();
		txtEmpPatronymic.Clear();
		txtEmpPosition.Clear();
		txtEmpPhone.Clear();
		comboBoxShops.SelectedValue = -1;
		dataGridViewEmployees.ClearSelection();
	}

	private bool ValidateRequiredFields()
	{
		if (string.IsNullOrWhiteSpace(txtEmpFirstName.Text))
		{
			MessageBox.Show("Имя сотрудника обязательно для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtEmpFirstName.Focus();
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtEmpLastName.Text))
		{
			MessageBox.Show("Фамилия сотрудника обязательна для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtEmpLastName.Focus();
			return false;
		}
		return true;
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		ClearFields();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.dataGridViewEmployees = new System.Windows.Forms.DataGridView();
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.txtEmpFirstName = new System.Windows.Forms.TextBox();
		this.txtEmpLastName = new System.Windows.Forms.TextBox();
		this.txtEmpPatronymic = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.txtEmpPosition = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.txtEmpPhone = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.btnAdd = new System.Windows.Forms.Button();
		this.comboBoxShops = new System.Windows.Forms.ComboBox();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewEmployees).BeginInit();
		base.SuspendLayout();
		this.dataGridViewEmployees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewEmployees.Location = new System.Drawing.Point(1, 63);
		this.dataGridViewEmployees.Name = "dataGridViewEmployees";
		this.dataGridViewEmployees.Size = new System.Drawing.Size(911, 307);
		this.dataGridViewEmployees.TabIndex = 0;
		this.btnUpdate.Location = new System.Drawing.Point(702, 399);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(85, 39);
		this.btnUpdate.TabIndex = 1;
		this.btnUpdate.Text = "Обновить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(815, 399);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(85, 39);
		this.btnDelete.TabIndex = 2;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.txtEmpFirstName.Location = new System.Drawing.Point(1, 34);
		this.txtEmpFirstName.Name = "txtEmpFirstName";
		this.txtEmpFirstName.Size = new System.Drawing.Size(100, 23);
		this.txtEmpFirstName.TabIndex = 3;
		this.txtEmpLastName.Location = new System.Drawing.Point(122, 34);
		this.txtEmpLastName.Name = "txtEmpLastName";
		this.txtEmpLastName.Size = new System.Drawing.Size(100, 23);
		this.txtEmpLastName.TabIndex = 3;
		this.txtEmpPatronymic.Location = new System.Drawing.Point(237, 34);
		this.txtEmpPatronymic.Name = "txtEmpPatronymic";
		this.txtEmpPatronymic.Size = new System.Drawing.Size(100, 23);
		this.txtEmpPatronymic.TabIndex = 3;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(26, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(31, 15);
		this.label1.TabIndex = 4;
		this.label1.Text = "Имя";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(140, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(58, 15);
		this.label2.TabIndex = 4;
		this.label2.Text = "Фамилия";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(259, 9);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(58, 15);
		this.label3.TabIndex = 4;
		this.label3.Text = "Отчество";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(618, 9);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(74, 15);
		this.label4.TabIndex = 4;
		this.label4.Text = "ID Магазина";
		this.txtEmpPosition.Location = new System.Drawing.Point(357, 34);
		this.txtEmpPosition.Name = "txtEmpPosition";
		this.txtEmpPosition.Size = new System.Drawing.Size(100, 23);
		this.txtEmpPosition.TabIndex = 3;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(370, 9);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(69, 15);
		this.label5.TabIndex = 4;
		this.label5.Text = "Должность";
		this.txtEmpPhone.Location = new System.Drawing.Point(475, 34);
		this.txtEmpPhone.Name = "txtEmpPhone";
		this.txtEmpPhone.Size = new System.Drawing.Size(100, 23);
		this.txtEmpPhone.TabIndex = 3;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(501, 9);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(45, 15);
		this.label6.TabIndex = 4;
		this.label6.Text = "Номер";
		this.btnAdd.Location = new System.Drawing.Point(716, 33);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(83, 24);
		this.btnAdd.TabIndex = 5;
		this.btnAdd.Text = "добавить";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.comboBoxShops.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxShops.FormattingEnabled = true;
		this.comboBoxShops.Location = new System.Drawing.Point(600, 33);
		this.comboBoxShops.Name = "comboBoxShops";
		this.comboBoxShops.Size = new System.Drawing.Size(110, 23);
		this.comboBoxShops.TabIndex = 6;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(912, 450);
		base.Controls.Add(this.comboBoxShops);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtEmpPhone);
		base.Controls.Add(this.txtEmpPosition);
		base.Controls.Add(this.txtEmpPatronymic);
		base.Controls.Add(this.txtEmpLastName);
		base.Controls.Add(this.txtEmpFirstName);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Controls.Add(this.dataGridViewEmployees);
		base.Name = "EmployeesForm";
		this.Text = "Employees";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewEmployees).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
