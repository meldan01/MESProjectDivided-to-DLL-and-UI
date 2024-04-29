using NewMasApp.ExternalComponents;
using NewMasApp.WorkEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MachineEntity = NewMasApp.WorkEntities.Machine;
using PartEntity = NewMasApp.WorkEntities.Part;
using WorkOrderEntity = NewMasApp.WorkEntities.WorkOrder;
using WorkEntities = NewMasApp.WorkEntities;


namespace UIForNewMesSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Logger m_logsInstance;
        public static DBConnectionManager m_dbConnection;
        private static bool m_machineSent = false, m_partSent = false, m_workOrderSent = false;
        public MainWindow()
        {
            InitializeComponent();
            startExternalComponents();
        }


        #region General

        private static void startExternalComponents()
        {
            m_logsInstance = Logger.getInstance();
            m_dbConnection = DBConnectionManager.getInstance();
        }


        public static bool validateCreationDate(DateTime? creationDate)
        {
            if (creationDate == null || !creationDate.HasValue)
                return false;
            return true;
        }


        private bool validateLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit) || languageCode.Length > 5))
                return false;
            else
                return true;
        }

        private bool validateUpdateLanguageCode(string languageCode)
        {
            if (!string.IsNullOrEmpty(languageCode) && !languageCode.All(char.IsDigit) || languageCode.Length > 5)
                return false;
            return true;
        }

        public static bool validateCreatorID(string creatorID)
        {
            if (string.IsNullOrEmpty(creatorID) || (creatorID.Length != 9) || !creatorID.All(char.IsDigit) || !idCheck(creatorID))
                return false;
            else
                return true;
        }

        public static bool validateUpdateCreator(string creatorID)
        {
            if (!string.IsNullOrEmpty(creatorID) && (creatorID.Length != 9 || !creatorID.All(char.IsDigit) || !idCheck(creatorID)))
                return false;
            return true;
        }

        public bool validateDBConnection()
        {
            if (m_dbConnection == null)
            {
                displayMachineMessage("Error - could not establish Database connection", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        #endregion General

        #region Machine
        private void saveMachine_Click(object sender, RoutedEventArgs e)
        {
            txtMachineSaveWarning.Visibility = Visibility.Collapsed;
            if (!validateDBConnection())
                return;
            if (validateMachineFields())
            {
                if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
                {
                    postMachineToDatabase();
                }
                else
                {
                    displayMachineMessage("Machine name already exist.", Brushes.Red, Visibility.Visible);
                }
            }
        }

        /// <summary>
        /// getMachinesInfo_Click - gets all the machines info from the DB
        /// and show it in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getMachinesInfo_Click(object sender, RoutedEventArgs e)
        {
            txtMachineSaveWarning.Visibility = Visibility.Collapsed;
            if (!validateDBConnection())
                return;
            txtMachineMessage.Text = WorkEntities.Machine.fetchMachinesInfo();
            displayMachineMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }

        /// <summary>
        /// updateMachine_Click - Updates the current machine .
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!validateDBConnection())
                return;
            if (!validateBeforeUpdate())
                return;
            if (MachineEntity.updateMachine(txtMachineName.Text, dpDateOfCreation.SelectedDate, txtCreatorID.Text, txtLanguageCode.Text))
            {
                displayMachineMessage($"Machine {txtMachineName.Text} successfully updated", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                displayMachineMessage($"Could not update machine {txtMachineName.Text}", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// deleteMachine_Click - deletes the current machine.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!validateDBConnection())
                return;
            if (!userDeleteValidation())
                return;
            if (MachineEntity.deleteMachine(txtMachineName.Text))
            {
                displayMachineMessage($"Machine {txtMachineName.Text} deleted successfully", Brushes.LightGreen, Visibility.Visible);
                m_logsInstance.Log($"Debug - Machine {txtMachineName.Text} deleted successfully.");
                deleteMachineWorkOrders(txtMachineName.Text);
                return;
            }
            else
            {
                displayMachineMessage($"Could not delete machine {txtMachineName.Text} from the Database", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Error - machine {txtMachineName.Text} not deleted.");
                return;
            }
        }


        /// <summary>
        /// setMachineMessage - Indicates user about state of the system
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void displayMachineMessage(string message, Brush foreground, Visibility visibility)
        {
            txtMachineSaveWarning.Text = message;
            txtMachineSaveWarning.Foreground = foreground;
            txtMachineSaveWarning.Visibility = visibility;
        }

        /// <summary>
        /// startSuccessfulMachineProtocol - Method that run after all check are done when sabe btn pressed
        /// </summary>
        private void postMachineToDatabase()
        {
            WorkEntities.Machine newMachine = new WorkEntities.Machine(dpDateOfCreation.SelectedDate.Value, txtCreatorID.Text, txtLanguageCode.Text, txtMachineName.Text);
            m_machineSent = newMachine.insertMachineIntoDB();
            logAndNotifyUser();
        }

        /// <summary>
        /// idCheck - Checks the id is a valid ID
        /// </summary>
        /// <param name="IDNumber"></param>
        /// <returns></returns>
        public static bool idCheck(string IDNumber)
        {
            int alter = 1;
            int total = 0;
            int temp = 0;
            char[] digit = new char[2];
            try
            {
                for (int i = 0; i < 9; i++)
                {
                    digit[0] = IDNumber[i];
                    temp = alter * int.Parse(new string(digit));
                    if (temp >= 10)
                        temp = temp - 9;
                    total = total + temp;
                    alter = (alter == 1) ? 2 : 1;
                }
            }
            catch (Exception ex)
            {
                m_logsInstance.Log(($"the id : {IDNumber} checked and found to be false"));
                m_logsInstance.Log("EXCEPTION" + ex.Message);
            }
            return total % 10 == 0;
        }

        /// <summary>
        /// logAndNotifyUser - notify the user if machine successfully added to DB
        /// </summary>
        private void logAndNotifyUser()
        {
            if (m_machineSent)
            {
                m_logsInstance.Log("Debug" + $" Machine number {txtMachineName.Text} successfully sent to the DB");
                displayMachineMessage($"Machine {txtMachineName.Text} successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $" - Machine {txtMachineName.Text} Could not be created due to unknown Error in the constructor");
                displayMachineMessage($"Machine {txtMachineName.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// CheckMachineFields - check that the properties of machine 
        /// are properly filled before sending to DB
        /// built in a way that one validation will end the function and save time.
        /// </summary>
        /// <returns></returns>
        private bool validateMachineFields()
        {
            if (!validateMachineName(txtMachineName.Text))
            {
                displayMachineMessage("Please enter a valid machine name (max 50 characters).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateCreatorID(txtCreatorID.Text))
            {
                displayMachineMessage("Please enter a valid creator ID (9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            else if (!validateLanguageCode(txtLanguageCode.Text))
            {
                displayMachineMessage("Please enter a valid language code (max 5 digits).", Brushes.Red, Visibility.Visible);
                return false;
            }
            else if (!validateCreationDate(dpDateOfCreation.SelectedDate))
            {
                displayMachineMessage("Please enter a creation date", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true; ;
        }

        /// <summary>
        /// validateBeforeUpdate - validations to the fields before update
        /// </summary>
        /// <returns></returns>
        private bool validateBeforeUpdate()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text) || txtMachineName.Text.Length > 50)
            {
                displayMachineMessage("Please enter a valid machine name (max 50 characters).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
            {
                displayMachineMessage($"Machine {txtMachineName.Text} not exist in the Database", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (isMachineFieldsEmpty())
            {
                displayMachineMessage($"Please fill at least on field to update.", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateUpdateCreator(txtCreatorID.Text))
            {
                displayMachineMessage($"Creator ID should be a valid ID(9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateUpdateLanguageCode(txtLanguageCode.Text))
            {
                displayMachineMessage("Please enter a valid language code (max 5 digits).", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// machineNameValidation - machineName validation
        /// </summary>
        /// <returns></returns>
        private bool validateMachineName(string machineName)
        {
            if (string.IsNullOrEmpty(machineName) || machineName.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// isMachineFieldsEmpty - Checks if all the machine fields are empty
        /// </summary>
        /// <returns></returns>
        private bool isMachineFieldsEmpty()
        {
            return string.IsNullOrEmpty(txtCreatorID.Text) &
             string.IsNullOrEmpty(txtLanguageCode.Text) &
             !dpDateOfCreation.SelectedDate.HasValue;
        }

        /// <summary>
        /// userDeleteValidation - validation before delete
        /// </summary>
        /// <returns></returns>
        private bool userDeleteValidation()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text))
            {
                displayMachineMessage("Please type machine name to delete", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!Machine.machineExists(txtMachineName.Text))
            {
                displayMachineMessage($"Could not find machine {txtMachineName.Text} in the Database", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Debug - machine {txtMachineName.Text} not found, therefore it will not be deleted");
                return false;
            }
            return true;
        }

        /// <summary>
        /// DeleteAllMachineRelatedWorkOrders - deletes all the work orders that related to the current deleted machine
        /// </summary>
        /// <param name="text"></param>
        private void deleteMachineWorkOrders(string text)
        {
            if (!MachineEntity.DeleteOrdersByMachineName(txtMachineName.Text))
            {
                m_logsInstance.Log($"Debug - No work orders that related to {txtMachineName.Text} deleted.");
            }
        }
        #endregion Machine


        #region Part


        /// <summary>
        /// SavePart_Click - Saves new part to the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePart_Click(object sender, RoutedEventArgs e)
        {
            txtPartSaveWarning.Visibility = Visibility.Collapsed;

            if (validatePartFields())
            {
                if (!WorkEntities.Part.partExists(txtCatalogID.Text))
                {
                    postPartToDatabase();
                }
                else
                {
                    displayPartMessage($"Catalog ID number {txtCatalogID.Text} already exist.", Brushes.Red, Visibility.Visible);
                }
            }
        }

        /// <summary>
        /// updatePart_Click - If the the part exists, updates it according to the fields that are not null 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updatePart_Click(object sender, RoutedEventArgs e)
        {
            if (!validatePartUpdateFields())
                return;
            if (PartEntity.updatePart(txtCatalogID.Text, txtItemDescription.Text, dpPartDateOfCreation.SelectedDate, txtPartCreatorID.Text, txtPartLanguageCode.Text))
                logAndDisplayPartUpdateSuccess();
        }


        /// <summary>
        /// getPartsInfo_Click - Gets the Part information from the DB and show it in the textBlock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPartsInfo_Click(object sender, RoutedEventArgs e)
        {
            txtPartSaveWarning.Visibility = Visibility.Collapsed;
            txtPartMessage.Text = PartEntity.fetchPartsInfo();
            displayPartMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }


        /// <summary>
        /// deletePart_Click - Deletes the current part 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deletePart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCatalogID.Text))
            {
                displayPartMessage("Please type a valid numeric catalog ID to delete", Brushes.Red, Visibility.Visible);
                return;
            }
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (PartEntity.deletePart(txtCatalogID.Text))
                {
                    displayPartMessage($"Part {txtCatalogID.Text} successfully deleted,", Brushes.LightGreen, Visibility.Visible);
                    m_logsInstance.Log($"Debug - part {txtCatalogID.Text} successfully deleted");
                    if (PartEntity.deleteWorkOrdersByCatalogID(txtCatalogID.Text))
                    {
                        m_logsInstance.Log($"Debug - deleted all orders related to {txtCatalogID.Text} successfully.");
                    }
                }
            }
            else
            {
                displayPartMessage($"Part {txtCatalogID.Text} not found in the DataBase", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Debug - Part {txtCatalogID.Text} not found in the DataBase");
            }
        }

        /// <summary>
        /// logAndDisplayPartUpdateSuccess - writes to the logs and to the UI about successful update
        /// </summary>
        private void logAndDisplayPartUpdateSuccess()
        {
            displayPartMessage($"Part {txtCatalogID.Text} successfully updated."
    , Brushes.LightGreen, Visibility.Visible);
            m_logsInstance.Log($"Debug - Part {txtCatalogID.Text} successfully updated.");
        }

        /// <summary>
        /// PartUpdateFieldsValidation - validates the fields of the part before updating
        /// </summary>
        /// <returns></returns>
        private bool validatePartUpdateFields()
        {
            if (string.IsNullOrEmpty(txtCatalogID.Text) || txtCatalogID.Text.Length > 50)
            {
                displayPartMessage("Please enter a valid numeric catalog ID(max 50 characters)."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (areAllPartFieldsEmpty())
                return false;
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (!descriptionMinLengthCheck())
                {
                    displayPartMessage("Please enter a description with at least 25 characters."
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
                if (!descriptionMaxLengthCheck())
                {
                    displayPartMessage("Please enter a description with at most 254 characters."
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
                if (!validateUpdateCreator(txtPartCreatorID.Text))
                {
                    displayPartMessage($"Please enter a valid ID (9 digits)"
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
                if (!validateUpdateLanguageCode(txtPartLanguageCode.Text))
                {
                    displayPartMessage("Please enter a valid language code(max 5 characters)."
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
            }
            else
            {
                displayPartMessage($"Part {txtCatalogID.Text} not exist in the DataBase."
                  , Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// areAllPartFieldsEmpty - returns true if all fields are empty
        /// </summary>
        /// <returns></returns>
        private bool areAllPartFieldsEmpty()
        {
            if (
            string.IsNullOrEmpty(txtItemDescription.Text) &
            string.IsNullOrEmpty(txtPartCreatorID.Text) &
            string.IsNullOrEmpty(txtPartLanguageCode.Text) &
            !dpPartDateOfCreation.SelectedDate.HasValue)
            {
                displayPartMessage($"Please fill at least one field to update."
                , Brushes.Red, Visibility.Visible);
                return true;
            }
            return false;
        }

        /// <summary>
        /// SetPartMessage - indicates the user about the system status by a print to the UI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void displayPartMessage(string message, Brush foreground, Visibility visibility)
        {
            txtPartSaveWarning.Text = message;
            txtPartSaveWarning.Foreground = foreground;
            txtPartSaveWarning.Visibility = visibility;
        }

        /// <summary>
        /// descriptionLengthValidation - makes sure that the user added some minimal proper description
        /// </summary>
        /// <returns></returns>
        private bool descriptionMinLengthCheck()
        {
            if (!string.IsNullOrEmpty(txtItemDescription.Text))
                if (txtItemDescription.Text.Length < 25)
                    return false;
            return true;
        }

        /// <summary>
        /// descriptionMaxLengthValidation - makes sure that the user added some proper description
        /// </summary>
        /// <returns></returns>
        private bool descriptionMaxLengthCheck()
        {
            if (!string.IsNullOrEmpty(txtItemDescription.Text))
                if (txtItemDescription.Text.Length > 254)
                    return false;
            return true;
        }

        /// <summary>
        /// startSuccessfulPartProtocol - After the validations start running the add to DB validation
        /// </summary>
        private void postPartToDatabase()
        {
            WorkEntities.Part newPart = new WorkEntities.Part(dpPartDateOfCreation.SelectedDate.Value, txtPartCreatorID.Text, txtPartLanguageCode.Text, txtCatalogID.Text, txtItemDescription.Text);
            m_partSent = newPart.insertPartIntoDB();
            logAndDisplayPartMessage();
        }

        /// <summary>
        /// writes to the logs and to UI about successful db insert
        /// </summary>
        private void logAndDisplayPartMessage()
        {
            if (m_partSent)
            {
                m_partSent = false;
                m_logsInstance.Log("Debug" + $"Part {txtCatalogID.Text} successfully sent to the DB");
                displayPartMessage($"Part {txtCatalogID.Text} successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $"Part {txtCatalogID.Text} Could not be created due to unknown Error in the constructor");
                displayPartMessage($"Error - Part {txtCatalogID.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// areAllPartFieldsValid - Validates fields in the UI are properly filled
        /// built in a way that one validation will end the function and save time.
        /// </summary>
        /// <returns></returns>
        private bool validatePartFields()
        {
            if (!validateSaveCatalogId(txtCatalogID.Text))
            {
                displayPartMessage("Please enter a valid numeric catalog ID(max 50 characters).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateMinPartDescription(txtItemDescription.Text))
            {
                displayPartMessage("Please enter a valid description(minimum 25 characters)."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateMaxPartDescription(txtItemDescription.Text))
            {
                displayPartMessage("Please enter a valid description(max 254 characters)."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateCreationDate(dpPartDateOfCreation.SelectedDate))
            {
                displayPartMessage("Please fill the date of creation field."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateCreatorID(txtPartCreatorID.Text))
            {
                displayPartMessage($"Creator ID should be a valid ID(9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateLanguageCode(txtPartLanguageCode.Text))
            {
                displayPartMessage("Please enter a valid language code(max 5 characters).", Brushes.Red, Visibility.Visible);
                return false;
            }

            return true;
        }

        /// <summary>
        /// partCatalogIdValidation - Validates the catalog ID field
        /// </summary>
        /// <returns></returns>
        private bool validateSaveCatalogId(string catalogID)
        {
            if (string.IsNullOrEmpty(catalogID) ||
                (!catalogID.All(char.IsDigit)) || catalogID.Length > 50)

                return false;
            else
                return true;
        }

        /// <summary>
        /// partDescriptionValidation - Validates the description field
        /// </summary>
        /// <returns></returns>
        private bool validateMinPartDescription(string description)
        {
            if (description.Length < 25)
                return false;
            else
                return true;
        }

        private bool validateMaxPartDescription(string description)
        {
            if (description.Length > 254)
                return false;
            else
                return true;
        }


        #endregion Part

        #region WorkOrder

        /// <summary>
        /// saveWorkOrder_Click - Saves the current workOrder to the DB 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            txtWorkOrderSaveWarning.Visibility = Visibility.Collapsed;
            if (validateAndNotifyOrderFields())
            {
                if (!WorkOrder.orderExists(txtOrderNumber.Text))
                {
                    if (!validateMachinePartExist())
                    {
                        displayWorkOrderMessage($"Machine or part not exist.", Brushes.Red, Visibility.Visible);
                        return;
                    }
                    initiateWorkOrderSave();
                }
                else
                {
                    displayWorkOrderMessage($"Work order number {txtOrderNumber.Text} already exist.", Brushes.Red, Visibility.Visible);
                }
            }
        }

        /// <summary>
        /// deleteWorkOrder_Click - Deletes the current workOrder 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!validateOrderNumber(txtOrderNumber.Text))
            {
                displayWorkOrderMessage("Please type a valid order number to delete", Brushes.Red, Visibility.Visible);
                return;
            }
            if (!WorkOrderEntity.orderExists(txtOrderNumber.Text))
            {
                displayWorkOrderMessage("Order number not found in the Database", Brushes.Red, Visibility.Visible);
                return;
            }
            if (WorkOrderEntity.deleteWorkOrder(txtOrderNumber.Text))
            {
                displayWorkOrderMessage($"Order number {txtOrderNumber.Text} deleted successfully", Brushes.LightGreen, Visibility.Visible);
                m_logsInstance.Log($"Order number {txtOrderNumber.Text} deleted successfully");
            }
            else
                displayWorkOrderMessage($"Could not delete order number {txtOrderNumber.Text}", Brushes.Red, Visibility.Visible);
        }

        /// <summary>
        /// updateWorkOrder_Click - Updates the current workOrder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtOrderNumber.Text))
            {
                displayWorkOrderMessage("Please enter a valid numeric order number.", Brushes.Red, Visibility.Visible);
                return;
            }
            if (areAllOrderFieldsEmpty())
                return;
            if (!validateAndNotifyUpdateOrderFields())
                return;
            try
            {
                bool updateSucceeded = WorkOrderEntity.updateWorkOrder(txtOrderNumber.Text, txtWorkOrderCatalogID.Text, textMachineName.Text, txtAmountToProduce.Text, dpDateOfCreationWorkOrder.SelectedDate,
                    txtOrderIDCreatorID.Text, txtWorkOrderLanguageCode.Text);
                if (updateSucceeded)
                    displayWorkOrderMessage($"Order {txtOrderNumber.Text} successfully updated", Brushes.LightGreen, Visibility.Visible);
                else
                {
                    displayWorkOrderMessage($"Could not update order {txtOrderNumber.Text}", Brushes.Red, Visibility.Visible);
                    m_logsInstance.Log("Debug - " + $"Could not update order {txtOrderNumber.Text}");
                }
            }
            catch (Exception ex)
            {
                m_logsInstance.Log("EXCEPTION" + ex.Message);
            }
        }

        /// <summary>
        /// getWorkOrdersInfo_Click - Gets and show in the UI the entire DB workOrders table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getWorkOrdersInfo_Click(object sender, RoutedEventArgs e)
        {
            txtWorkOrderSaveWarning.Visibility = Visibility.Collapsed;
            txtWorkOrderMessage.Text = WorkOrderEntity.fetchOrdersInfo();
            displayWorkOrderMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }

        /// <summary>
        /// validateMachinePartExist - validations that machine and part exist
        /// used to check before adding work order
        /// </summary>
        /// <returns></returns>
        private bool validateMachinePartExist()
        {
            if (!(Machine.machineExists(textMachineName.Text)))
            {
                displayWorkOrderMessage("Machine not exist", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log("Debug" + $" - Machine or Part not exist or order number exist, therefore order {txtOrderNumber.Text}  will not be added to DB");
                return false;
            }
            if (!Part.partExists(txtWorkOrderCatalogID.Text))
            {
                displayWorkOrderMessage("Part not exist", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log("Debug" + $" - Machine or Part not exist or order number exist, therefore order {txtOrderNumber.Text}  will not be added to DB");
                return false;
            }
            return true;
        }

        /// <summary>
        /// startSuccessfulWorkOrderProtocol - After all the validations 
        /// </summary>
        private void initiateWorkOrderSave()
        {
            WorkEntities.WorkOrder newWorkOrder = new WorkEntities.WorkOrder(dpDateOfCreationWorkOrder.SelectedDate.Value, txtOrderIDCreatorID.Text, txtWorkOrderLanguageCode.Text, txtOrderNumber.Text, txtWorkOrderCatalogID.Text, textMachineName.Text, txtAmountToProduce.Text);
            m_workOrderSent = newWorkOrder.insertWorkOrderIntoDB();
            if (m_workOrderSent)
            {
                m_logsInstance.Log("Debug" + $"Work order number {txtOrderNumber.Text} successfully sent to the DB");
                displayWorkOrderMessage("Work order successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $" - Work order {txtOrderNumber.Text} Could not be created due to unknown Error in the constructor");
                displayPartMessage($"Error - Work order {txtOrderNumber.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// validateAndNotifyOrderFields - The next function returns true if all the fields in work order section
        /// filled properly, otherwise false
        /// built in a way that one validation will end the function and save time.
        /// Use only when save pressed and not update
        /// </summary>
        /// <returns></returns>
        private bool validateAndNotifyOrderFields()
        {
            if (!validateOrderNumber(txtOrderNumber.Text))
            {
                displayWorkOrderMessage("Please enter a valid numeric order number(max 50 digits).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateOrderCatalogID(txtWorkOrderCatalogID.Text))
            {
                displayWorkOrderMessage("Please enter a valid numeric catalog ID(max 50 digits).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateOrderMachineName(textMachineName.Text))
            {
                displayWorkOrderMessage("Please enter a valid machine name (maximum 50 characters).", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateOrderQuantity(txtAmountToProduce.Text))
            {
                displayWorkOrderMessage("Please enter a valid numeric quantity(maximum 50 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateCreationDate(dpDateOfCreationWorkOrder.SelectedDate))
            {
                displayWorkOrderMessage("Please enter a valid creation date.", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateCreatorID(txtOrderIDCreatorID.Text))
            {
                displayWorkOrderMessage("Please enter a valid numerical creator ID(9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateLanguageCode(txtWorkOrderLanguageCode.Text))
            {
                displayWorkOrderMessage("Please enter a valid numerical language code(max 5 digits).", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// orderNumberValidation - Validates the order number field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderNumber(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) ||
                (!orderNumber.All(char.IsDigit)) || orderNumber.Length > 50)
            {
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// catalogIDValidation - validates the catalog ID field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderCatalogID(string catalogID)
        {
            if (string.IsNullOrEmpty(catalogID) ||
                (!catalogID.All(char.IsDigit)) || catalogID.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// amountToProduceValidation -  Validates the quantity field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderQuantity(string quantity)
        {
            if (string.IsNullOrEmpty(quantity) ||
                (!quantity.All(char.IsDigit)) || quantity.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// validateUpdateOrderQuantity -  Validates the quantity field in case of update
        /// </summary>
        /// <returns></returns>
        private bool validateUpdateOrderQuantity(string quantity)
        {
            if (!string.IsNullOrEmpty(quantity) &&
                (!quantity.All(char.IsDigit)) || quantity.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineNameOrderValidation - Validates the machine name field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderMachineName(string machineName)
        {
            if (string.IsNullOrEmpty(machineName) || machineName.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// validateAndNotifyUpdateOrderFields - validations before update
        /// </summary>
        /// <returns></returns>
        private bool validateAndNotifyUpdateOrderFields()
        {
            if (!validateUpdateCreator(txtOrderIDCreatorID.Text))
            {
                displayWorkOrderMessage("Please enter a valid numerical creator ID(9 digits)"
    , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateUpdateLanguageCode(txtWorkOrderLanguageCode.Text))
            {
                displayWorkOrderMessage("Please enter a valid numerical language code(max 5 digits)."
    , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!validateUpdateOrderQuantity(txtAmountToProduce.Text))
            {
                displayWorkOrderMessage("Please enter a valid numeric quantity(max 50 digits)"
    , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!isUpdateMachineExist())
                return false;
            if (!isUpdatePartExist())
                return false;
            return true;
        }

        private bool isUpdatePartExist()
        {
            if (!string.IsNullOrEmpty(txtWorkOrderCatalogID.Text) && !Part.partExists(txtWorkOrderCatalogID.Text))
            {
                displayWorkOrderMessage($"Part {txtWorkOrderCatalogID.Text} not exist", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log("Debug" + $" - Machine or Part not exist or order number exist, therefore order {txtOrderNumber.Text}  will not be added to DB");
                return false;
            }
            return true;
        }
        
        private bool isUpdateMachineExist()
        {
            if (!string.IsNullOrEmpty(textMachineName.Text) &&  !(Machine.machineExists(textMachineName.Text)))
            {
                displayWorkOrderMessage($"Machine {textMachineName.Text} not exist", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log("Debug" + $" - Machine or Part not exist or order number exist, therefore order {txtOrderNumber.Text}  will not be added to DB");
                return false;
            }
            return true;
        }

        /// <summary>
        /// isAllOrderUIFieldsEmpty - return true if all the order fields are null or empty
        /// in case of datepicker if it empty 
        /// </summary>
        /// <returns></returns>
        private bool areAllOrderFieldsEmpty()
        {
            if (string.IsNullOrEmpty(txtWorkOrderCatalogID.Text) &
             string.IsNullOrEmpty(textMachineName.Text) &
             string.IsNullOrEmpty(txtAmountToProduce.Text) &
             string.IsNullOrEmpty(txtOrderIDCreatorID.Text) &
             string.IsNullOrEmpty(txtWorkOrderLanguageCode.Text) &
             !dpDateOfCreationWorkOrder.SelectedDate.HasValue)
            {
                displayWorkOrderMessage("Please fill at list one field to update", Brushes.Red, Visibility.Visible);
                return true;
            }
            return false;
        }


        /// <summary>
        /// setWorkOrderMessage - Set messages to the UI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void displayWorkOrderMessage(string message, Brush foreground, Visibility visibility)
        {
            txtWorkOrderSaveWarning.Text = message;
            txtWorkOrderSaveWarning.Foreground = foreground;
            txtWorkOrderSaveWarning.Visibility = visibility;
        }
        #endregion WorkOrder
    }
}
