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

        /// <summary>
        /// isUpdatedSetter - A flag setter funcion 
        /// </summary>
        /// <param name="isSuccessfulUpdate"></param>
        #endregion General

        #region Machine
        private void saveMachine_Click(object sender, RoutedEventArgs e)
        {
            txtMachineSaveWarning.Visibility = Visibility.Collapsed;
            if (validateMachineFields())
            {
                if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
                {
                    postMachineToDatabase();
                }
                else
                {
                    displayWorkMachineMessage("Machine name already exist.", Brushes.Red, Visibility.Visible);
                }
            }
            else
            {
                displayWorkMachineMessage("Invalid input detected. Please ensure all fields" +
                    " are filled correctly to proceed.", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// setMachineMessage - Indicates user about state of the system
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void displayWorkMachineMessage(string message, Brush foreground, Visibility visibility)
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
            newMachine.insertMachineIntoDB();
            logAndNotifyUser();
        }

        private void logAndNotifyUser()
        {
            if (m_machineSent)
            {
                m_machineSent = false;
                m_logsInstance.Log("Debug" + $" Machine number {txtMachineName.Text} successfully sent to the DB");
                displayWorkMachineMessage($"Machine {txtMachineName.Text} successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $" - Machine {txtMachineName.Text} Could not be created due to unknown Error in the constructor");
                displayWorkMachineMessage($"Machine {txtMachineName.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
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
                return false;
            if (!validateMachineCreatorId(txtCreatorID.Text))
                return false;
            if (!validateMachineLanguageCode(txtLanguageCode.Text))
                return false;
            if (!machineDpValidation(dpDateOfCreation.SelectedDate))
                return false;
            return true; ;
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
            txtMachineMessage.Text = WorkEntities.Machine.fetchMachinesInfo();
            displayWorkMachineMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }

        /// <summary>
        /// updateMachine_Click - Updates the current machine .
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!validateBeforeUpdate())
                return;
            if (MachineEntity.updateMachine(txtMachineName.Text, dpDateOfCreation.SelectedDate, txtCreatorID.Text, txtLanguageCode.Text))
            {
                displayWorkMachineMessage($"Machine {txtMachineName.Text} successfully updated", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                displayWorkMachineMessage($"Could not update machine {txtMachineName.Text}", Brushes.Red, Visibility.Visible);
            }
        }

        private bool validateBeforeUpdate()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text))
            {
                displayWorkMachineMessage($"Please type a valid machine name to update", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
            {
                displayWorkMachineMessage($"Machine {txtMachineName.Text} not exist in the Database", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (isMachineFieldsEmpty())
            {
                displayWorkMachineMessage($"Please fill at least on field to update.", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (isUpdateMachineFieldsValid(txtCreatorID.Text))
            {
                displayWorkMachineMessage($"Creator ID should be a valid ID(9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// isUpdateMachineFieldsValid - validates the fields of machine before update
        /// </summary>
        /// <param name="creatorID"></param>
        /// <returns></returns>
        public static bool isUpdateMachineFieldsValid(string creatorID)
        {
            return (!string.IsNullOrEmpty(creatorID) && (creatorID.Length != 9 || !creatorID.All(char.IsDigit)));
        }

        /// <summary>
        /// machineNameValidation - machineName validation
        /// </summary>
        /// <returns></returns>
        private bool validateMachineName(string machineName)
        {
            if (string.IsNullOrEmpty(machineName))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineCreatorIdValidation - validation to the creator ID
        /// </summary>
        /// <returns></returns>
        private bool validateMachineCreatorId(string creatorID)
        {
            if (string.IsNullOrEmpty(creatorID) || (creatorID.Length != 9) || !creatorID.All(char.IsDigit))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineLanguageCodeValidation - validation of the language code of machine
        /// </summary>
        /// <returns></returns>
        private bool validateMachineLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineDpValidation - validation of the datePicker
        /// </summary>
        /// <returns></returns>
        private bool machineDpValidation(DateTime? creationDate)
        {
            if (creationDate == null)
                return false;
            if (!creationDate.HasValue)
                return false;
            else
                return true;
        }

        private bool isMachineFieldsEmpty()
        {
            return string.IsNullOrEmpty(txtCreatorID.Text) &
             string.IsNullOrEmpty(txtLanguageCode.Text) &
             !dpDateOfCreation.SelectedDate.HasValue;
        }

        /// <summary>
        /// deleteMachine_Click - deletes the current machine.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!userDeleteValidation())
                return;
            if (MachineEntity.deleteMachine(txtMachineName.Text))
            {
                displayWorkMachineMessage($"Machine {txtMachineName.Text} deleted successfully", Brushes.LightGreen, Visibility.Visible);
                m_logsInstance.Log($"Debug - Machine {txtMachineName.Text} deleted successfully.");
                deleteMachineWorkOrders(txtMachineName.Text);
                return;
            }
            else
            {
                displayWorkMachineMessage($"Could not delete machine {txtMachineName.Text} from the Database", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Error - machine {txtMachineName.Text} not deleted.");
                return;
            }
        }

        private bool userDeleteValidation()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text))
            {
                displayWorkMachineMessage("Please type machine name to delete", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!Machine.machineExists(txtMachineName.Text))
            {
                displayWorkMachineMessage($"Could not find machine {txtMachineName.Text} in the Database", Brushes.Red, Visibility.Visible);
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
            else
            {
                displayPartMessage("Invalid input detected. Please ensure all fields are filled correctly to proceed."
                    , Brushes.Red, Visibility.Visible);
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
            else
                logAndDisplayPartUpdateFailure();
        }

        private void logAndDisplayPartUpdateFailure()
        {
            displayPartMessage($"Could not update part {txtCatalogID.Text}."
    , Brushes.Red, Visibility.Visible);
            m_logsInstance.Log($"Error - Could not update part {txtCatalogID.Text}.");
        }

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
            if (string.IsNullOrEmpty(txtCatalogID.Text))
            {
                displayPartMessage($"Please enter catalog ID."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (areAllPartFieldsEmpty())
                return false;
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (!descriptionLengthCheck())
                    return false;
                if (!partCreatorUpdateValidation())
                    return false;
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
        /// partCreatorUpdateValidation - in update creator can be null or empty 
        /// therefore we check it only if its not empty
        /// </summary>
        /// <returns></returns>
        private bool partCreatorUpdateValidation()
        {
            if (!string.IsNullOrEmpty(txtPartCreatorID.Text) && ((txtPartCreatorID.Text.Length != 9) || !txtPartCreatorID.Text.All(char.IsDigit)))
            {
                displayPartMessage($"Creator ID should be a valid ID(9 digits)"
                    , Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

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
        /// descriptionLengthValidation - makes sure that the user added some proper description
        /// </summary>
        /// <returns></returns>
        private bool descriptionLengthCheck()
        {
            if (!string.IsNullOrEmpty(txtItemDescription.Text))
            {
                if (txtItemDescription.Text.Length < 25)
                {
                    displayPartMessage("Please enter a description with at least 25 characters."
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
            }
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
            if (!validatePartCatalogId(txtCatalogID.Text))
                return false;
            if (!validatePartDescription(txtItemDescription.Text))
                return false;
            if (!validatePartDp(dpPartDateOfCreation.SelectedDate))
                return false;
            if (!validatePartCreatorId(txtPartCreatorID.Text))
                return false;
            if (!validatePartLanguageCode(txtPartLanguageCode.Text))
                return false;
            return true;
        }

        /// <summary>
        /// partCatalogIdValidation - Validates the catalog ID field
        /// </summary>
        /// <returns></returns>
        private bool validatePartCatalogId(string catalogID)
        {
            if (string.IsNullOrEmpty(catalogID) ||
                (!catalogID.All(char.IsDigit)))

                return false;
            else
                return true;
        }

        /// <summary>
        /// partDescriptionValidation - Validates the description field
        /// </summary>
        /// <returns></returns>
        private bool validatePartDescription(string description)
        {
            if (description.Length < 50)//minimal description
                return false;
            else
                return true;
        }

        /// <summary>
        /// partDpValidation - Validates the part datePicker field
        /// </summary>
        /// <returns></returns>
        private bool validatePartDp(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
                return false;
            else
                return true;
        }

        /// <summary>
        /// partCreatorIdValidation - Validates the creator field
        /// </summary>
        /// <returns></returns>
        private bool validatePartCreatorId(string creatorID)
        {
            if (string.IsNullOrEmpty(creatorID) || (creatorID.Length != 9) || !creatorID.All(char.IsDigit))
                return false;
            else
                return true;
        }

        /// <summary>
        /// partLanguageCodeValidation - Validates the language code field
        /// </summary>
        /// <returns></returns>
        private bool validatePartLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
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
                displayPartMessage("Please type a valid catalog ID to delete", Brushes.Red, Visibility.Visible);
                return;
            }
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (PartEntity.deletePart(txtCatalogID.Text))
                {
                    displayPartMessage($"Part {txtCatalogID.Text} successfully deleted,", Brushes.LightGreen, Visibility.Visible);
                    m_logsInstance.Log($"Debug - part {txtCatalogID.Text} successfully deleted");
                    if (PartEntity.deletePart(txtCatalogID.Text))
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
            if (areAllWorkOrderFieldsValid())
            {
                if (!WorkOrder.orderExists(txtOrderNumber.Text))
                {
                    if (!validateMachinePartOrder())
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
            else
            {
                displayWorkOrderMessage("Invalid input detected. Please ensure all fields are filled correctly to proceed.", Brushes.Red, Visibility.Visible);
            }
        }

        private bool validateMachinePartOrder()
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
                m_workOrderSent = false;
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
        /// The next function returns true if all the fields in work order section
        /// filled properly, otherwise false
        /// built in a way that one validation will end the function and save time.
        /// </summary>
        /// <returns></returns>
        private bool areAllWorkOrderFieldsValid()
        {
            if (!validateOrderNumber(txtOrderNumber.Text))
                return false;
            if (!validateOrderCatalogID(txtWorkOrderCatalogID.Text))
                return false;
            if (!validateOrderMachineName(textMachineName.Text))
                return false;
            if (!validateOrderQuantity(txtAmountToProduce.Text))
                return false;
            if (!validateOrderDPWork(dpDateOfCreationWorkOrder.SelectedDate))
                return false;
            if (!validateOrderCreatorID(txtOrderIDCreatorID.Text))
                return false;
            if (!validateOrderLanguageCode(txtWorkOrderLanguageCode.Text))
                return false;
            return true;
        }

        /// <summary>
        /// orderNumberValidation - Validates the order number field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderNumber(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) ||
                (!orderNumber.All(char.IsDigit)))
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
                (!catalogID.All(char.IsDigit)))
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
                (!quantity.All(char.IsDigit)))
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
        /// dpWorkOrderValidation - Validates datepicker field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderDPWork(DateTime? creationDate)
        {
            if ((!creationDate.HasValue))
                return false;
            else
                return true;
        }

        /// <summary>
        /// creatorIDWorkOrderValidation -  Validates the creator ID field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderCreatorID(string creatorID)
        {
            if ((creatorID.Length != 9) ||
                (!creatorID.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// languageCodeWorkOrderValidation - Validates the language code field
        /// </summary>
        /// <returns></returns>
        private bool validateOrderLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
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
            if (WorkOrderEntity.deleteWorkOrderByOrderNumber(txtOrderNumber.Text))
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
                displayWorkOrderMessage("Please type order number", Brushes.Red, Visibility.Visible);
                return;
            }
            if (areAllOrderFieldsEmpty())
                return;
            if (!validateOrderCreatorUpdate())
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

        private bool validateOrderCreatorUpdate()
        {
            if (!string.IsNullOrEmpty(txtOrderIDCreatorID.Text) && ((txtOrderIDCreatorID.Text.Length != 9) || !txtOrderIDCreatorID.Text.All(char.IsDigit)))
            {
                displayWorkOrderMessage($"Creator ID should be a valid ID(9 digits)"
                    , Brushes.Red, Visibility.Visible);
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
