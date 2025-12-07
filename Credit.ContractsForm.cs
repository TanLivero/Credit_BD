// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.ContractsForm
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class ContractsForm : Form
{
	public class ComboBoxItem
	{
		public int Id { get; set; }

		public string Text { get; set; }

		public ComboBoxItem(int id, string text)
		{
			Id = id;
			Text = text;
		}

		public override string ToString()
		{
			return Text;
		}
	}

	private DatabaseHelper dbHelper;

	private DataTable dataTable;

	private DataTable clientsData;

	private DataTable productsData;

	private DataTable employeesData;

	private DataTable shopsData;

	private IContainer components = null;

	private DataGridView dataGridViewContracts;

	private DateTimePicker dtpContractDate;

	private NumericUpDown numLoanAmount;

	private NumericUpDown numInitialPayment;

	private NumericUpDown numInterestRate;

	private NumericUpDown numLoanTermMonths;

	private NumericUpDown numMonthlyPayment;

	private NumericUpDown numTotalPaid;

	private NumericUpDown numRemainingDebt;

	private Button btnUpdate;

	private Button btnDelete;

	private ComboBox comboClientId;

	private ComboBox comboProductId;

	private ComboBox comboEmployeeId;

	private ComboBox comboShopId;

	private Button btnAdd;

	private Button btnRefreshReferences;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	private Label label10;

	private Label label11;

	private Label label12;

	public ContractsForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadReferenceData();
		LoadData();
		SetupForm();
		btnAdd.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		btnRefreshReferences.Click += btnRefreshReferences_Click;
		dataGridViewContracts.SelectionChanged += dataGridViewContracts_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewContracts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewContracts.ReadOnly = true;
		dataGridViewContracts.MultiSelect = false;
		dataGridViewContracts.AllowUserToAddRows = false;
		dataGridViewContracts.AllowUserToDeleteRows = false;
		dtpContractDate.Value = DateTime.Now;
		SetupNumericUpDownControls();
		SetupReferenceComboBoxes();
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void SetupNumericUpDownControls()
	{
		if (numLoanAmount != null)
		{
			numLoanAmount.Minimum = 1m;
			numLoanAmount.Maximum = 100000000m;
			numLoanAmount.DecimalPlaces = 2;
			numLoanAmount.Increment = 1000m;
			numLoanAmount.Value = 10000m;
		}
		if (numInitialPayment != null)
		{
			numInitialPayment.Minimum = 0m;
			numInitialPayment.Maximum = 100000000m;
			numInitialPayment.DecimalPlaces = 2;
			numInitialPayment.Increment = 1000m;
			numInitialPayment.Value = 0m;
		}
		if (numInterestRate != null)
		{
			numInterestRate.Minimum = 0.01m;
			numInterestRate.Maximum = 100m;
			numInterestRate.DecimalPlaces = 2;
			numInterestRate.Increment = 0.1m;
			numInterestRate.Value = 10.0m;
		}
		if (numLoanTermMonths != null)
		{
			numLoanTermMonths.Minimum = 1m;
			numLoanTermMonths.Maximum = 360m;
			numLoanTermMonths.Increment = 1m;
			numLoanTermMonths.Value = 12m;
		}
		if (numMonthlyPayment != null)
		{
			numMonthlyPayment.Minimum = 1m;
			numMonthlyPayment.Maximum = 10000000m;
			numMonthlyPayment.DecimalPlaces = 2;
			numMonthlyPayment.Increment = 100m;
			numMonthlyPayment.Value = 1000m;
		}
		if (numTotalPaid != null)
		{
			numTotalPaid.Minimum = 0m;
			numTotalPaid.Maximum = 100000000m;
			numTotalPaid.DecimalPlaces = 2;
			numTotalPaid.Increment = 1000m;
			numTotalPaid.Value = 0m;
		}
		if (numRemainingDebt != null)
		{
			numRemainingDebt.Minimum = 0m;
			numRemainingDebt.Maximum = 100000000m;
			numRemainingDebt.DecimalPlaces = 2;
			numRemainingDebt.Increment = 1000m;
			numRemainingDebt.Value = 10000m;
		}
	}

	private void LoadReferenceData()
	{
		try
		{
			string clientsQuery = "SELECT client_id, first_name || ' ' || last_name as full_name FROM Clients ORDER BY last_name, first_name";
			clientsData = dbHelper.ExecuteQuery(clientsQuery);
			string productsQuery = "SELECT product_id, name FROM Products ORDER BY name";
			productsData = dbHelper.ExecuteQuery(productsQuery);
			string employeesQuery = "SELECT employee_id, first_name || ' ' || last_name as full_name FROM Employees ORDER BY last_name, first_name";
			employeesData = dbHelper.ExecuteQuery(employeesQuery);
			string shopsQuery = "SELECT shop_id, name FROM Shops ORDER BY name";
			shopsData = dbHelper.ExecuteQuery(shopsQuery);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки справочных данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetupReferenceComboBoxes()
	{
		comboClientId.Items.Clear();
		comboClientId.Items.Add("-- Выберите клиента --");
		if (clientsData != null && clientsData.Rows.Count > 0)
		{
			foreach (DataRow row2 in clientsData.Rows)
			{
				int clientId = Convert.ToInt32(row2["client_id"]);
				string clientName = row2["full_name"].ToString();
				comboClientId.Items.Add(new ComboBoxItem(clientId, clientName));
			}
		}
		comboClientId.SelectedIndex = 0;
		comboProductId.Items.Clear();
		comboProductId.Items.Add("-- Выберите продукт --");
		if (productsData != null && productsData.Rows.Count > 0)
		{
			foreach (DataRow row4 in productsData.Rows)
			{
				int productId = Convert.ToInt32(row4["product_id"]);
				string productName = row4["name"].ToString();
				comboProductId.Items.Add(new ComboBoxItem(productId, productName));
			}
		}
		comboProductId.SelectedIndex = 0;
		comboEmployeeId.Items.Clear();
		comboEmployeeId.Items.Add("-- Не выбрано --");
		if (employeesData != null && employeesData.Rows.Count > 0)
		{
			foreach (DataRow row3 in employeesData.Rows)
			{
				int employeeId = Convert.ToInt32(row3["employee_id"]);
				string employeeName = row3["full_name"].ToString();
				comboEmployeeId.Items.Add(new ComboBoxItem(employeeId, employeeName));
			}
		}
		comboEmployeeId.SelectedIndex = 0;
		comboShopId.Items.Clear();
		comboShopId.Items.Add("-- Не выбрано --");
		if (shopsData != null && shopsData.Rows.Count > 0)
		{
			foreach (DataRow row in shopsData.Rows)
			{
				int shopId = Convert.ToInt32(row["shop_id"]);
				string shopName = row["name"].ToString();
				comboShopId.Items.Add(new ComboBoxItem(shopId, shopName));
			}
		}
		comboShopId.SelectedIndex = 0;
	}

	private int GetSelectedIdFromComboBox(ComboBox comboBox)
	{
		if (comboBox.SelectedIndex <= 0 || comboBox.SelectedItem == null)
		{
			return -1;
		}
		if (comboBox.SelectedItem is ComboBoxItem item)
		{
			return item.Id;
		}
		return -1;
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT cc.*, \r\n                                        c.first_name || ' ' || c.last_name as client_name,\r\n                                        p.name as product_name,\r\n                                        e.first_name || ' ' || e.last_name as employee_name,\r\n                                        s.name as shop_name\r\n                                 FROM Credit_Contracts cc\r\n                                 LEFT JOIN Clients c ON cc.client_id = c.client_id\r\n                                 LEFT JOIN Products p ON cc.product_id = p.product_id\r\n                                 LEFT JOIN Employees e ON cc.employee_id = e.employee_id\r\n                                 LEFT JOIN Shops s ON cc.shop_id = s.shop_id\r\n                                 ORDER BY cc.contract_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewContracts.DataSource = dataTable;
			ConfigureDataGridViewColumns();
			HideTechnicalColumns();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void ConfigureDataGridViewColumns()
	{
		if (dataGridViewContracts.Columns.Contains("client_name"))
		{
			dataGridViewContracts.Columns["client_name"].HeaderText = "Клиент";
		}
		if (dataGridViewContracts.Columns.Contains("product_name"))
		{
			dataGridViewContracts.Columns["product_name"].HeaderText = "Продукт";
		}
		if (dataGridViewContracts.Columns.Contains("employee_name"))
		{
			dataGridViewContracts.Columns["employee_name"].HeaderText = "Сотрудник";
		}
		if (dataGridViewContracts.Columns.Contains("shop_name"))
		{
			dataGridViewContracts.Columns["shop_name"].HeaderText = "Магазин";
		}
		if (dataGridViewContracts.Columns.Contains("contract_date"))
		{
			dataGridViewContracts.Columns["contract_date"].HeaderText = "Дата договора";
		}
		if (dataGridViewContracts.Columns.Contains("loan_amount"))
		{
			dataGridViewContracts.Columns["loan_amount"].HeaderText = "Сумма кредита";
		}
		if (dataGridViewContracts.Columns.Contains("initial_payment"))
		{
			dataGridViewContracts.Columns["initial_payment"].HeaderText = "Первоначальный взнос";
		}
		if (dataGridViewContracts.Columns.Contains("interest_rate"))
		{
			dataGridViewContracts.Columns["interest_rate"].HeaderText = "Процентная ставка";
		}
		if (dataGridViewContracts.Columns.Contains("loan_term_months"))
		{
			dataGridViewContracts.Columns["loan_term_months"].HeaderText = "Срок (мес.)";
		}
		if (dataGridViewContracts.Columns.Contains("monthly_payment"))
		{
			dataGridViewContracts.Columns["monthly_payment"].HeaderText = "Ежемесячный платеж";
		}
		if (dataGridViewContracts.Columns.Contains("total_paid"))
		{
			dataGridViewContracts.Columns["total_paid"].HeaderText = "Всего оплачено";
		}
		if (dataGridViewContracts.Columns.Contains("remaining_debt"))
		{
			dataGridViewContracts.Columns["remaining_debt"].HeaderText = "Остаток долга";
		}
		if (dataGridViewContracts.Columns.Contains("status"))
		{
			dataGridViewContracts.Columns["status"].HeaderText = "Статус";
		}
		if (dataGridViewContracts.Columns.Contains("loan_amount"))
		{
			dataGridViewContracts.Columns["loan_amount"].DefaultCellStyle.Format = "N2";
		}
		if (dataGridViewContracts.Columns.Contains("initial_payment"))
		{
			dataGridViewContracts.Columns["initial_payment"].DefaultCellStyle.Format = "N2";
		}
		if (dataGridViewContracts.Columns.Contains("interest_rate"))
		{
			dataGridViewContracts.Columns["interest_rate"].DefaultCellStyle.Format = "N2";
		}
		if (dataGridViewContracts.Columns.Contains("monthly_payment"))
		{
			dataGridViewContracts.Columns["monthly_payment"].DefaultCellStyle.Format = "N2";
		}
		if (dataGridViewContracts.Columns.Contains("total_paid"))
		{
			dataGridViewContracts.Columns["total_paid"].DefaultCellStyle.Format = "N2";
		}
		if (dataGridViewContracts.Columns.Contains("remaining_debt"))
		{
			dataGridViewContracts.Columns["remaining_debt"].DefaultCellStyle.Format = "N2";
		}
	}

	private void HideTechnicalColumns()
	{
		string[] columnsToHide = new string[5] { "contract_id", "client_id", "product_id", "employee_id", "shop_id" };
		string[] array = columnsToHide;
		foreach (string columnName in array)
		{
			if (dataGridViewContracts.Columns.Contains(columnName))
			{
				dataGridViewContracts.Columns[columnName].Visible = false;
			}
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (!ValidateRequiredFields())
			{
				return;
			}
			int clientId = GetSelectedIdFromComboBox(comboClientId);
			int productId = GetSelectedIdFromComboBox(comboProductId);
			int employeeId = GetSelectedIdFromComboBox(comboEmployeeId);
			int shopId = GetSelectedIdFromComboBox(comboShopId);
			if (clientId == -1 || productId == -1)
			{
				MessageBox.Show("Клиент и продукт обязательны для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			string query = "INSERT INTO Credit_Contracts \r\n                        (client_id, product_id, employee_id, shop_id, contract_date, \r\n                         loan_amount, initial_payment, interest_rate, loan_term_months, \r\n                         monthly_payment, total_paid, remaining_debt, status) \r\n                         VALUES (@clientId, @productId, @employeeId, @shopId, @contractDate, \r\n                         @loanAmount, @initialPayment, @interestRate, @loanTermMonths, \r\n                         @monthlyPayment, @totalPaid, @remainingDebt, @status::contract_status)";
			object employeeIdValue = ((employeeId == -1) ? DBNull.Value : ((object)employeeId));
			object shopIdValue = ((shopId == -1) ? DBNull.Value : ((object)shopId));
			NpgsqlParameter[] parameters = new NpgsqlParameter[13]
			{
				new NpgsqlParameter("@clientId", clientId),
				new NpgsqlParameter("@productId", productId),
				new NpgsqlParameter("@employeeId", employeeIdValue),
				new NpgsqlParameter("@shopId", shopIdValue),
				new NpgsqlParameter("@contractDate", dtpContractDate.Value.Date),
				new NpgsqlParameter("@loanAmount", numLoanAmount.Value),
				new NpgsqlParameter("@initialPayment", numInitialPayment.Value),
				new NpgsqlParameter("@interestRate", numInterestRate.Value),
				new NpgsqlParameter("@loanTermMonths", (int)numLoanTermMonths.Value),
				new NpgsqlParameter("@monthlyPayment", numMonthlyPayment.Value),
				new NpgsqlParameter("@totalPaid", numTotalPaid.Value),
				new NpgsqlParameter("@remainingDebt", numRemainingDebt.Value),
				new NpgsqlParameter("@status", "Активен")
			};
			int result = dbHelper.ExecuteNonQuery(query, parameters);
			if (result > 0)
			{
				LoadData();
				ClearFields();
				MessageBox.Show("Кредитный договор добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				MessageBox.Show("Не удалось добавить кредитный договор!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении кредитного договора: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewContracts.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int contractId = Convert.ToInt32(dataGridViewContracts.CurrentRow.Cells["contract_id"].Value);
					int clientId = GetSelectedIdFromComboBox(comboClientId);
					int productId = GetSelectedIdFromComboBox(comboProductId);
					int employeeId = GetSelectedIdFromComboBox(comboEmployeeId);
					int shopId = GetSelectedIdFromComboBox(comboShopId);
					if (clientId == -1 || productId == -1)
					{
						MessageBox.Show("Клиент и продукт обязательны для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					else
					{
						string query = "UPDATE Credit_Contracts SET \r\n                            client_id=@clientId, product_id=@productId, employee_id=@employeeId, \r\n                            shop_id=@shopId, contract_date=@contractDate, \r\n                            loan_amount=@loanAmount, initial_payment=@initialPayment, \r\n                            interest_rate=@interestRate, loan_term_months=@loanTermMonths, \r\n                            monthly_payment=@monthlyPayment, total_paid=@totalPaid, \r\n                            remaining_debt=@remainingDebt, status=@status::contract_status \r\n                            WHERE contract_id=@contractId";
						object employeeIdValue = ((employeeId == -1) ? DBNull.Value : ((object)employeeId));
						object shopIdValue = ((shopId == -1) ? DBNull.Value : ((object)shopId));
						NpgsqlParameter[] parameters = new NpgsqlParameter[14]
						{
							new NpgsqlParameter("@clientId", clientId),
							new NpgsqlParameter("@productId", productId),
							new NpgsqlParameter("@employeeId", employeeIdValue),
							new NpgsqlParameter("@shopId", shopIdValue),
							new NpgsqlParameter("@contractDate", dtpContractDate.Value.Date),
							new NpgsqlParameter("@loanAmount", numLoanAmount.Value),
							new NpgsqlParameter("@initialPayment", numInitialPayment.Value),
							new NpgsqlParameter("@interestRate", numInterestRate.Value),
							new NpgsqlParameter("@loanTermMonths", (int)numLoanTermMonths.Value),
							new NpgsqlParameter("@monthlyPayment", numMonthlyPayment.Value),
							new NpgsqlParameter("@totalPaid", numTotalPaid.Value),
							new NpgsqlParameter("@remainingDebt", numRemainingDebt.Value),
							new NpgsqlParameter("@status", "Активен"),
							new NpgsqlParameter("@contractId", contractId)
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
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите кредитный договор для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewContracts.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранный кредитный договор?\nВсе связанные платежи также будут удалены!", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int contractId = Convert.ToInt32(dataGridViewContracts.CurrentRow.Cells["contract_id"].Value);
				string query = "DELETE FROM Credit_Contracts WHERE contract_id=@contractId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@contractId", contractId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Кредитный договор удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить кредитный договор!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении кредитного договора: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите кредитный договор для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void dataGridViewContracts_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewContracts.CurrentRow != null && dataGridViewContracts.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (hasSelection)
		{
			DataGridViewRow row = dataGridViewContracts.CurrentRow;
			SetComboBoxValue(comboClientId, row.Cells["client_id"].Value);
			SetComboBoxValue(comboProductId, row.Cells["product_id"].Value);
			SetComboBoxValue(comboEmployeeId, row.Cells["employee_id"].Value);
			SetComboBoxValue(comboShopId, row.Cells["shop_id"].Value);
			if (row.Cells["contract_date"].Value != null && row.Cells["contract_date"].Value != DBNull.Value)
			{
				dtpContractDate.Value = Convert.ToDateTime(row.Cells["contract_date"].Value);
			}
			SetSafeNumericValue(numLoanAmount, row.Cells["loan_amount"].Value, 1m);
			SetSafeNumericValue(numInitialPayment, row.Cells["initial_payment"].Value, 0m);
			SetSafeNumericValue(numInterestRate, row.Cells["interest_rate"].Value, 0.01m);
			SetSafeNumericValue(numLoanTermMonths, row.Cells["loan_term_months"].Value, 1m);
			SetSafeNumericValue(numMonthlyPayment, row.Cells["monthly_payment"].Value, 1m);
			SetSafeNumericValue(numTotalPaid, row.Cells["total_paid"].Value, 0m);
			SetSafeNumericValue(numRemainingDebt, row.Cells["remaining_debt"].Value, 0m);
		}
	}

	private void SetComboBoxValue(ComboBox comboBox, object value)
	{
		if (value == null || value == DBNull.Value)
		{
			comboBox.SelectedIndex = 0;
			return;
		}
		int id = Convert.ToInt32(value);
		for (int i = 0; i < comboBox.Items.Count; i++)
		{
			if (comboBox.Items[i] is ComboBoxItem item && item.Id == id)
			{
				comboBox.SelectedIndex = i;
				return;
			}
		}
		comboBox.SelectedIndex = 0;
	}

	private void SetSafeNumericValue(NumericUpDown numericControl, object value, decimal defaultValue)
	{
		if (numericControl == null)
		{
			return;
		}
		if (value == null || value == DBNull.Value)
		{
			numericControl.Value = defaultValue;
			return;
		}
		try
		{
			decimal numericValue = Convert.ToDecimal(value);
			if (numericValue < numericControl.Minimum)
			{
				numericValue = numericControl.Minimum;
			}
			else if (numericValue > numericControl.Maximum)
			{
				numericValue = numericControl.Maximum;
			}
			numericControl.Value = numericValue;
		}
		catch
		{
			numericControl.Value = defaultValue;
		}
	}

	private void ClearFields()
	{
		comboClientId.SelectedIndex = 0;
		comboProductId.SelectedIndex = 0;
		comboEmployeeId.SelectedIndex = 0;
		comboShopId.SelectedIndex = 0;
		dtpContractDate.Value = DateTime.Now;
		numLoanAmount.Value = 10000m;
		numInitialPayment.Value = 0m;
		numInterestRate.Value = 10.0m;
		numLoanTermMonths.Value = 12m;
		numMonthlyPayment.Value = 1000m;
		numTotalPaid.Value = 0m;
		numRemainingDebt.Value = 10000m;
		if (dataGridViewContracts != null)
		{
			dataGridViewContracts.ClearSelection();
		}
	}

	private bool ValidateRequiredFields()
	{
		if (comboClientId.SelectedIndex <= 0)
		{
			MessageBox.Show("Выберите клиента!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			comboClientId.Focus();
			return false;
		}
		if (comboProductId.SelectedIndex <= 0)
		{
			MessageBox.Show("Выберите продукт!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			comboProductId.Focus();
			return false;
		}
		if (numLoanAmount.Value <= 0m)
		{
			MessageBox.Show("Сумма кредита должна быть положительным числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numLoanAmount.Focus();
			return false;
		}
		if (numInitialPayment.Value < 0m)
		{
			MessageBox.Show("Первоначальный взнос не может быть отрицательным!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numInitialPayment.Focus();
			return false;
		}
		if (numInterestRate.Value < 0.01m || numInterestRate.Value > 100m)
		{
			MessageBox.Show("Процентная ставка должна быть от 0.01 до 100!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numInterestRate.Focus();
			return false;
		}
		if (numLoanTermMonths.Value <= 0m)
		{
			MessageBox.Show("Срок кредита должен быть положительным числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numLoanTermMonths.Focus();
			return false;
		}
		if (numMonthlyPayment.Value <= 0m)
		{
			MessageBox.Show("Ежемесячный платеж должен быть положительным числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numMonthlyPayment.Focus();
			return false;
		}
		if (numRemainingDebt.Value < 0m)
		{
			MessageBox.Show("Остаток долга не может быть отрицательным!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			numRemainingDebt.Focus();
			return false;
		}
		return true;
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		ClearFields();
	}

	private void btnRefreshReferences_Click(object sender, EventArgs e)
	{
		try
		{
			LoadReferenceData();
			SetupReferenceComboBoxes();
			MessageBox.Show("Справочные данные обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при обновлении справочных данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnCalculateMonthlyPayment_Click(object sender, EventArgs e)
	{
		try
		{
			decimal loanAmount = numLoanAmount.Value;
			decimal interestRate = numInterestRate.Value;
			int loanTermMonths = (int)numLoanTermMonths.Value;
			if (loanAmount > 0m && interestRate > 0m && loanTermMonths > 0)
			{
				decimal monthlyRate = interestRate / 100m / 12m;
				decimal monthlyPayment = loanAmount * (monthlyRate * (decimal)Math.Pow((double)(1m + monthlyRate), loanTermMonths)) / (decimal)(Math.Pow((double)(1m + monthlyRate), loanTermMonths) - 1.0);
				numMonthlyPayment.Value = Math.Round(monthlyPayment, 2);
				numRemainingDebt.Value = loanAmount;
				MessageBox.Show($"Расчетный ежемесячный платеж: {monthlyPayment:C}", "Расчет", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				MessageBox.Show("Заполните сумму кредита, процентную ставку и срок!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при расчете платежа: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
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
		this.dataGridViewContracts = new System.Windows.Forms.DataGridView();
		this.dtpContractDate = new System.Windows.Forms.DateTimePicker();
		this.numLoanAmount = new System.Windows.Forms.NumericUpDown();
		this.numInitialPayment = new System.Windows.Forms.NumericUpDown();
		this.numInterestRate = new System.Windows.Forms.NumericUpDown();
		this.numLoanTermMonths = new System.Windows.Forms.NumericUpDown();
		this.numMonthlyPayment = new System.Windows.Forms.NumericUpDown();
		this.numTotalPaid = new System.Windows.Forms.NumericUpDown();
		this.numRemainingDebt = new System.Windows.Forms.NumericUpDown();
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.comboClientId = new System.Windows.Forms.ComboBox();
		this.comboProductId = new System.Windows.Forms.ComboBox();
		this.comboEmployeeId = new System.Windows.Forms.ComboBox();
		this.comboShopId = new System.Windows.Forms.ComboBox();
		this.btnAdd = new System.Windows.Forms.Button();
		this.btnRefreshReferences = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewContracts).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numLoanAmount).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numInitialPayment).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numInterestRate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numLoanTermMonths).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numMonthlyPayment).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numTotalPaid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numRemainingDebt).BeginInit();
		base.SuspendLayout();
		this.dataGridViewContracts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewContracts.Location = new System.Drawing.Point(1, 108);
		this.dataGridViewContracts.Name = "dataGridViewContracts";
		this.dataGridViewContracts.Size = new System.Drawing.Size(1991, 390);
		this.dataGridViewContracts.TabIndex = 0;
		this.dtpContractDate.Location = new System.Drawing.Point(1, 79);
		this.dtpContractDate.Name = "dtpContractDate";
		this.dtpContractDate.Size = new System.Drawing.Size(200, 23);
		this.dtpContractDate.TabIndex = 1;
		this.numLoanAmount.Location = new System.Drawing.Point(207, 79);
		this.numLoanAmount.Name = "numLoanAmount";
		this.numLoanAmount.Size = new System.Drawing.Size(120, 23);
		this.numLoanAmount.TabIndex = 2;
		this.numInitialPayment.Location = new System.Drawing.Point(333, 79);
		this.numInitialPayment.Name = "numInitialPayment";
		this.numInitialPayment.Size = new System.Drawing.Size(120, 23);
		this.numInitialPayment.TabIndex = 2;
		this.numInterestRate.Location = new System.Drawing.Point(459, 79);
		this.numInterestRate.Name = "numInterestRate";
		this.numInterestRate.Size = new System.Drawing.Size(120, 23);
		this.numInterestRate.TabIndex = 2;
		this.numLoanTermMonths.Location = new System.Drawing.Point(585, 79);
		this.numLoanTermMonths.Name = "numLoanTermMonths";
		this.numLoanTermMonths.Size = new System.Drawing.Size(120, 23);
		this.numLoanTermMonths.TabIndex = 2;
		this.numMonthlyPayment.Location = new System.Drawing.Point(711, 79);
		this.numMonthlyPayment.Name = "numMonthlyPayment";
		this.numMonthlyPayment.Size = new System.Drawing.Size(120, 23);
		this.numMonthlyPayment.TabIndex = 2;
		this.numTotalPaid.Location = new System.Drawing.Point(837, 79);
		this.numTotalPaid.Name = "numTotalPaid";
		this.numTotalPaid.Size = new System.Drawing.Size(120, 23);
		this.numTotalPaid.TabIndex = 2;
		this.numRemainingDebt.Location = new System.Drawing.Point(963, 79);
		this.numRemainingDebt.Name = "numRemainingDebt";
		this.numRemainingDebt.Size = new System.Drawing.Size(120, 23);
		this.numRemainingDebt.TabIndex = 2;
		this.btnUpdate.Location = new System.Drawing.Point(1836, 504);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(75, 45);
		this.btnUpdate.TabIndex = 3;
		this.btnUpdate.Text = "изменить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(1917, 504);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(75, 45);
		this.btnDelete.TabIndex = 4;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.comboClientId.FormattingEnabled = true;
		this.comboClientId.Location = new System.Drawing.Point(1089, 79);
		this.comboClientId.Name = "comboClientId";
		this.comboClientId.Size = new System.Drawing.Size(121, 23);
		this.comboClientId.TabIndex = 5;
		this.comboProductId.FormattingEnabled = true;
		this.comboProductId.Location = new System.Drawing.Point(1216, 78);
		this.comboProductId.Name = "comboProductId";
		this.comboProductId.Size = new System.Drawing.Size(121, 23);
		this.comboProductId.TabIndex = 5;
		this.comboEmployeeId.FormattingEnabled = true;
		this.comboEmployeeId.Location = new System.Drawing.Point(1343, 78);
		this.comboEmployeeId.Name = "comboEmployeeId";
		this.comboEmployeeId.Size = new System.Drawing.Size(121, 23);
		this.comboEmployeeId.TabIndex = 5;
		this.comboShopId.FormattingEnabled = true;
		this.comboShopId.Location = new System.Drawing.Point(1470, 78);
		this.comboShopId.Name = "comboShopId";
		this.comboShopId.Size = new System.Drawing.Size(121, 23);
		this.comboShopId.TabIndex = 5;
		this.btnAdd.Location = new System.Drawing.Point(1677, 77);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(75, 23);
		this.btnAdd.TabIndex = 6;
		this.btnAdd.Text = "Добавить";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.btnRefreshReferences.Location = new System.Drawing.Point(1597, 77);
		this.btnRefreshReferences.Name = "btnRefreshReferences";
		this.btnRefreshReferences.Size = new System.Drawing.Size(75, 23);
		this.btnRefreshReferences.TabIndex = 7;
		this.btnRefreshReferences.Text = "обновить";
		this.btnRefreshReferences.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(76, 52);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(32, 15);
		this.label1.TabIndex = 8;
		this.label1.Text = "Дата";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(218, 52);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(91, 15);
		this.label2.TabIndex = 8;
		this.label2.Text = "Сумма кредита";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(371, 52);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(39, 15);
		this.label3.TabIndex = 8;
		this.label3.Text = "Взнос";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(496, 52);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(53, 15);
		this.label4.TabIndex = 8;
		this.label4.Text = "процент";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(619, 52);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(35, 15);
		this.label5.TabIndex = 8;
		this.label5.Text = "Срок";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(728, 52);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(85, 15);
		this.label6.TabIndex = 8;
		this.label6.Text = "Ежемесечный";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(859, 52);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(65, 15);
		this.label7.TabIndex = 8;
		this.label7.Text = "Всего опл.";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(980, 52);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(74, 15);
		this.label8.TabIndex = 8;
		this.label8.Text = "Остаток дол";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(1126, 52);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(46, 15);
		this.label9.TabIndex = 8;
		this.label9.Text = "Клиент";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(1242, 52);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(53, 15);
		this.label10.TabIndex = 8;
		this.label10.Text = "Продукт";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(1364, 52);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(66, 15);
		this.label11.TabIndex = 8;
		this.label11.Text = "Сотрудник";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(1494, 52);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(54, 15);
		this.label12.TabIndex = 8;
		this.label12.Text = "Магазин";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1993, 691);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.btnRefreshReferences);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.comboShopId);
		base.Controls.Add(this.comboEmployeeId);
		base.Controls.Add(this.comboProductId);
		base.Controls.Add(this.comboClientId);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Controls.Add(this.numRemainingDebt);
		base.Controls.Add(this.numTotalPaid);
		base.Controls.Add(this.numMonthlyPayment);
		base.Controls.Add(this.numLoanTermMonths);
		base.Controls.Add(this.numInterestRate);
		base.Controls.Add(this.numInitialPayment);
		base.Controls.Add(this.numLoanAmount);
		base.Controls.Add(this.dtpContractDate);
		base.Controls.Add(this.dataGridViewContracts);
		base.Name = "ContractsForm";
		this.Text = "Contracts";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewContracts).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numLoanAmount).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numInitialPayment).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numInterestRate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numLoanTermMonths).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numMonthlyPayment).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numTotalPaid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numRemainingDebt).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
