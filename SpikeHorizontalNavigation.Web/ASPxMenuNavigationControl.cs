﻿using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Templates.ActionContainers;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.Templates;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;
using DevExpress.Web;

namespace SpikeHorizontalNavigation.Web
{
    public class CreateCustomGroupControlEventArgs : EventArgs
    {
        public CreateCustomGroupControlEventArgs(ChoiceActionItem groupItem)
        {
            GroupItem = groupItem;
        }

        public ChoiceActionItem GroupItem { get; private set; }

        public Control Control { get; set; }
    }
    public class ASPxMenuGroupChoiceActionItem : ChoiceActionItemWrapper
    {
        private readonly DevExpress.Web.MenuItem CurrentMenuGroup;
        public override void SetImageName(string imageName)
        {
            ASPxImageHelper.SetImageProperties(CurrentMenuGroup.Image, imageName);
        }
        public override void SetCaption(string caption)
        {
            CurrentMenuGroup.Text = caption;
            CurrentMenuGroup.Name = caption;
        }

        public override void SetData(object data) { }
        public override void SetShortcut(string shortcutString) { }
        public override void SetEnabled(bool enabled) { }

        public override void SetVisible(bool visible)
        {
            CurrentMenuGroup.Visible = visible;
        }
        public override void SetToolTip(string toolTip)
        {
            CurrentMenuGroup.ToolTip = toolTip;
        }
        public ASPxMenuGroupChoiceActionItem(SingleChoiceAction action, ChoiceActionItem item)
            : base(item, action)
        {
            CurrentMenuGroup = new DevExpress.Web.MenuItem();
            if (action.Items.IndexOf(item) == 0)
            {
                //CurrentMenuGroup.HeaderStyle.CssClass += " FirstHeader";
                //CurrentMenuGroup.HeaderStyleCollapsed.CssClass = " FirstHeaderCollapsed";
            }
            if (action.Items.IndexOf(item) == action.Items.Count - 1)
            {
                //CurrentMenuGroup.HeaderStyle.CssClass = " LastHeader";
                //CurrentMenuGroup.HeaderStyleCollapsed.CssClass = " LastHeaderCollapsed";
            }
            //CurrentMenuGroup.Expanded = false;
            SyncronizeWithItem();
        }
        public DevExpress.Web.MenuItem MenuGroup
        {
            get { return CurrentMenuGroup; }
        }
    }
    public class ASPxMenuItemChoiceActionItem : ChoiceActionItemWrapper
    {
        private readonly SingleChoiceAction CurrentAction;
        public override void SetImageName(string imageName)
        {
            ASPxImageHelper.SetImageProperties(CurrentMenuItem.Image, imageName);
        }
        public override void SetCaption(string caption)
        {
            CurrentMenuItem.Text = caption;
        }

        public override void SetData(object data) { }
        public override void SetShortcut(string shortcutString) { }

        public override void SetEnabled(bool enabled)
        {
            CurrentMenuItem.Enabled = enabled;
            CurrentMenuItem.ClientEnabled = enabled;
        }
        public override void SetVisible(bool visible)
        {
            CurrentMenuItem.Visible = visible;
        }
        public override void SetToolTip(string toolTip)
        {
            CurrentMenuItem.ToolTip = toolTip;
        }
        public ASPxMenuItemChoiceActionItem(SingleChoiceAction action, ChoiceActionItem item)
            : base(item, action)
        {
            CurrentAction = action;
            CurrentMenuItem = new DevExpress.Web.MenuItem { Name = item.GetIdPath() };
            //SyncronizeWithItem();
            SynchronizeWithItem();
        }
        public DevExpress.Web.MenuItem CurrentMenuItem { get; private set; }

        public void ExecuteAction()
        {
            if (CurrentAction.Active && CurrentAction.Enabled)
            {
                CurrentAction.DoExecute(ActionItem);
            }
        }
    }

    internal class ASPxMenuCustomControlItem : DevExpress.Web.MenuItem, IDisposable
    {
        private readonly Control control;
        public ASPxMenuCustomControlItem(Control control)
        {
            this.control = control;
        }

        internal void InitTemplate()
        {
            Template = new ASPxMenuCustomGroupControlTemplate(control);
        }
        #region IDisposable Members
        public void Dispose()
        {
            if (control != null)
            {
                ((IDisposable)control).Dispose();
            }
        }
        #endregion
        internal class ASPxMenuCustomGroupControlTemplate : ITemplate, IDisposable
        {
            private readonly Control control;
            public ASPxMenuCustomGroupControlTemplate(Control control)
            {
                this.control = control;
            }
            #region ITemplate Members
            public void InstantiateIn(Control container)
            {
                container.Controls.Add(control);
            }
            #endregion
            #region IDisposable Members
            public void Dispose()
            {
                if (control != null)
                {
                    ((IDisposable)control).Dispose();
                }
            }
            #endregion
        }
    }
    public class ASPxMenuNavigationControl : IWebNavigationControl, INavigationControlTestable, IDisposable, ISupportCallbackStartupScriptRegistering, IXafCallbackHandler/*, ISupportAdditionalParametersTestControl*/
    {
        private readonly ASPxMenu ASPxMenuControl;
        private readonly Dictionary<ChoiceActionItem, DevExpress.Web.MenuItem> ActionItemToMenuItemMap;
        private readonly LightDictionary<ChoiceActionItem, DevExpress.Web.MenuItem> ActionItemToMenuGroupMap;
        private readonly Dictionary<DevExpress.Web.MenuItem, ASPxMenuItemChoiceActionItem> MenuItemToWrapperMap;
        private readonly List<ASPxMenuGroupChoiceActionItem> GroupWrappers;
        private ChoiceActionItemCollection ActionItems;
        internal SingleChoiceAction SingleChoiceAction;

        public ASPxMenuNavigationControl()
        {
            ASPxMenuControl = RenderHelper.CreateASPxMenu();
            ASPxMenuControl.AllowSelectItem = true;
            ASPxMenuControl.Border.BorderStyle = BorderStyle.None;
            ASPxMenuControl.ID = "NB";
            ASPxMenuControl.ItemClick += ASPxMenuControl_ItemClick;
            ASPxMenuControl.Load += ASPxMenuControl_Load;
            ActionItemToMenuItemMap = new Dictionary<ChoiceActionItem, DevExpress.Web.MenuItem>();
            ActionItemToMenuGroupMap = new LightDictionary<ChoiceActionItem, DevExpress.Web.MenuItem>();
            MenuItemToWrapperMap = new Dictionary<DevExpress.Web.MenuItem, ASPxMenuItemChoiceActionItem>();
            GroupWrappers = new List<ASPxMenuGroupChoiceActionItem>();
        }

        private void BuildControl()
        {
            ActionItemToMenuItemMap.Clear();
            ActionItemToMenuGroupMap.Clear();
            ClearItemWrappers();
            ASPxMenuControl.Items.Clear();
            if (ActionItems.Count > 0)
            {
                FillMenuContents(ASPxMenuControl.Items, ActionItems);
            }
            UpdateSelection();
        }

        private void FillMenuContents(DevExpress.Web.MenuItemCollection holderMenuItems, ChoiceActionItemCollection actionItems)
        {
            foreach (ChoiceActionItem groupValue in actionItems)
            {
                if (!groupValue.Active)
                {
                    continue;
                }

                if (groupValue.Items.Count == 0)
                {
                    ASPxMenuItemChoiceActionItem itemWrapper = new ASPxMenuItemChoiceActionItem(SingleChoiceAction,
                        groupValue);
                    MenuItemToWrapperMap.Add(itemWrapper.CurrentMenuItem, itemWrapper);
                    holderMenuItems.Add(itemWrapper.CurrentMenuItem);
                    ActionItemToMenuItemMap.Add(groupValue, itemWrapper.CurrentMenuItem);
                }
                else
                {
                    ASPxMenuGroupChoiceActionItem groupItem = new ASPxMenuGroupChoiceActionItem(SingleChoiceAction,
                        groupValue);
                    GroupWrappers.Add(groupItem);
                    DevExpress.Web.MenuItem group = groupItem.MenuGroup;
                    ActionItemToMenuGroupMap.Add(groupValue, group);
                    holderMenuItems.Add(group);
                    ItemsDisplayStyle itemsDisplayStyle = ItemsDisplayStyle.LargeIcons;
                    if (groupValue.Model != null)
                    {
                        itemsDisplayStyle = ItemsDisplayStyle.List;
                        if (groupValue.Model is IModelChoiceActionItemChildItemsDisplayStyle)
                        {
                            itemsDisplayStyle =
                                ((IModelChoiceActionItemChildItemsDisplayStyle)groupValue.Model).ChildItemsDisplayStyle;
                        }
                    }
                    CreateCustomGroupControlEventArgs args = new CreateCustomGroupControlEventArgs(groupValue);
                    OnCreateCustomGroupControl(args);
                    if (args.Control != null)
                    {
                        var customControlItem = new ASPxMenuCustomControlItem(args.Control);
                        customControlItem.InitTemplate();
                        group.Items.Add(customControlItem);
                    }
                    else
                    {
                        switch (itemsDisplayStyle)
                        {
                            case ItemsDisplayStyle.LargeIcons:
                            case ItemsDisplayStyle.List:
                                if (groupValue.IsHierarchical())
                                {
                                    FillMenuContents(group.Items, groupValue.Items);
                                }
                                else
                                {
                                    FillMenuGroup(group, groupValue);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void FillMenuGroup(DevExpress.Web.MenuItem group, ChoiceActionItem groupValue)
        {
            foreach (ChoiceActionItem itemValue in groupValue.Items)
            {
                if (!itemValue.Active)
                {
                    continue;
                }
                ASPxMenuItemChoiceActionItem itemWrapper = new ASPxMenuItemChoiceActionItem(SingleChoiceAction, itemValue);
                MenuItemToWrapperMap.Add(itemWrapper.CurrentMenuItem, itemWrapper);
                group.Items.Add(itemWrapper.CurrentMenuItem);
                ActionItemToMenuItemMap.Add(itemValue, itemWrapper.CurrentMenuItem);
            }
        }

        private void singleChoiceAction_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            BuildControl();
        }

        private void UpdateSelection()
        {
            ASPxMenuControl.SelectedItem = null;
            if (SingleChoiceAction != null && SingleChoiceAction.SelectedItem != null)
            {
                ChoiceActionItem actionItem = SingleChoiceAction.SelectedItem;
                if (ActionItemToMenuItemMap.ContainsKey(actionItem))
                {
                    DevExpress.Web.MenuItem itemLink = ActionItemToMenuItemMap[actionItem];
                    ASPxMenuControl.SelectedItem = itemLink;
                }
            }
        }

        private void action_SelectedItemChanged(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        private void SubscribeToAction()
        {
            SingleChoiceAction.ItemsChanged += singleChoiceAction_ItemsChanged;
            SingleChoiceAction.SelectedItemChanged += action_SelectedItemChanged;
        }

        private void UnsubscribeFromAction()
        {
            if (SingleChoiceAction != null)
            {
                SingleChoiceAction.SelectedItemChanged -= action_SelectedItemChanged;
                SingleChoiceAction.ItemsChanged -= singleChoiceAction_ItemsChanged;
            }
        }

        private void UnsubscribeAll()
        {
            UnsubscribeFromAction();
            SingleChoiceAction = null;
            ActionItemToMenuItemMap.Clear();
            ActionItemToMenuGroupMap.Clear();
        }

        private DevExpress.Web.MenuItem FindMenuGroupControl(ChoiceActionItem item)
        {
            if (item != null && ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return ActionItemToMenuGroupMap[item];
            }
            return null;
        }

        private void ClearItemWrappers()
        {
            foreach (ASPxMenuItemChoiceActionItem itemWrapper in MenuItemToWrapperMap.Values)
            {
                itemWrapper.Dispose();
            }
            MenuItemToWrapperMap.Clear();
            foreach (ASPxMenuGroupChoiceActionItem groupWrapper in GroupWrappers)
            {
                groupWrapper.Dispose();
            }
            GroupWrappers.Clear();
        }

        protected virtual void OnCreateCustomGroupControl(CreateCustomGroupControlEventArgs args)
        {
            if (CreateCustomGroupControl != null)
            {
                CreateCustomGroupControl(this, args);
            }
        }

        protected virtual void OnRegisterCallbackStartupScript(RegisterCallbackStartupScriptEventArgs e)
        {
            if (RegisterCallbackStartupScript != null)
            {
                RegisterCallbackStartupScript(this, e);
            }
        }

        void ASPxMenuControl_ItemClick(object source, MenuItemEventArgs e)
        {
            if (e.Item != null && MenuItemToWrapperMap.ContainsKey(e.Item))
            {
                MenuItemToWrapperMap[e.Item].ExecuteAction();
            }
        }

        void ASPxMenuControl_Load(object sender, EventArgs e)
        {
            ICallbackManagerHolder holder = ASPxMenuControl.Page as ICallbackManagerHolder;
            if (holder != null)
            {
                holder.CallbackManager.RegisterHandler(ASPxMenuControl, this);
                ASPxMenuControl.ClientSideEvents.ItemClick = @"function(s,e) {
					e.processOnServer = false;" +
                    holder.CallbackManager.GetScript(ASPxMenuControl.UniqueID, "e.item.name", String.Empty, SingleChoiceAction.Model.GetValue<bool>("IsPostBackRequired")) +
                @"}";
            }
        }

        public event EventHandler<CreateCustomGroupControlEventArgs> CreateCustomGroupControl;

        #region INavigationControl
        public void SetNavigationActionItems(ChoiceActionItemCollection actionItems, SingleChoiceAction action)
        {
            Guard.ArgumentNotNull(action, "action");
            UnsubscribeFromAction();
            ActionItems = actionItems;
            SingleChoiceAction = action;
            BuildControl();
            SubscribeToAction();
        }
        public Control Control
        {
            get { return ASPxMenuControl; }
        }
        public Control TestControl
        {
            get { return ASPxMenuControl; }
        }
        #endregion

        #region ISupportNavigationActionContainerTesting
        public bool IsItemControlVisible(ChoiceActionItem item)
        {
            bool result = false;
            if (ActionItemToMenuGroupMap[item] != null)
            {
                result = ActionItemToMenuGroupMap[item].Visible;
            }
            return result;
        }
        public int GetGroupCount()
        {
            return ASPxMenuControl.Items.Count;
        }
        public string GetGroupControlCaption(ChoiceActionItem item)
        {
            if (ActionItemToMenuGroupMap[item] != null)
            {
                return ActionItemToMenuGroupMap[item].Text;
            }
            throw new ArgumentOutOfRangeException();
        }
        public int GetGroupChildControlCount(ChoiceActionItem item)
        {
            if (ActionItemToMenuGroupMap[item] != null)
            {
                return ActionItemToMenuGroupMap[item].Items.Count;
            }
            throw new ArgumentOutOfRangeException();
        }
        public string GetChildControlCaption(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap[item] != null)
            {
                return ActionItemToMenuItemMap[item].Text;
            }
            throw new ArgumentOutOfRangeException();
        }
        public bool GetChildControlEnabled(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap[item] != null)
            {
                return ActionItemToMenuItemMap[item].ClientEnabled;
            }
            throw new ArgumentOutOfRangeException();
        }
        public bool GetChildControlVisible(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap[item] != null)
            {
                return ActionItemToMenuItemMap[item].Visible;
            }
            throw new ArgumentOutOfRangeException();
        }
        public bool IsGroupExpanded(ChoiceActionItem item)
        {
            if (ActionItemToMenuGroupMap[item] != null)
            {
                return false;
            }
            throw new ArgumentOutOfRangeException();
        }
        public string GetSelectedItemCaption()
        {
            if (ASPxMenuControl.SelectedItem != null)
            {
                return ASPxMenuControl.SelectedItem.Text;
            }
            return string.Empty;
        }
        #endregion

        #region ISupportNavigationActionContainerTesting
        #endregion

        #region INavigationControlTestable Members
        bool INavigationControlTestable.IsItemEnabled(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap.ContainsKey(item))
            {
                return ActionItemToMenuItemMap[item].Enabled;
            }

            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return ActionItemToMenuGroupMap[item].Visible;
            }
            return false;
        }
        bool INavigationControlTestable.IsItemVisible(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap.ContainsKey(item))
            {
                return ActionItemToMenuItemMap[item].Visible;
            }

            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return ActionItemToMenuGroupMap[item].Visible;
            }
            return false;
        }
        int INavigationControlTestable.GetSubItemsCount(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap.ContainsKey(item))
            {
                return 0;
            }

            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                DevExpress.Web.MenuItem menuGroup = ActionItemToMenuGroupMap[item];
                if (menuGroup.Items.Count > 0)
                {
                    return menuGroup.Items[0].Items.Count;
                }
                return ActionItemToMenuGroupMap[item].Items.Count;
            }
            return 0;
        }
        string INavigationControlTestable.GetItemCaption(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap.ContainsKey(item))
            {
                return ActionItemToMenuItemMap[item].Text;
            }

            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return ActionItemToMenuGroupMap[item].Text;
            }
            return string.Empty;
        }
        string INavigationControlTestable.GetItemToolTip(ChoiceActionItem item)
        {
            if (ActionItemToMenuItemMap.ContainsKey(item))
            {
                return ActionItemToMenuItemMap[item].ToolTip;
            }
            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return ActionItemToMenuGroupMap[item].ToolTip;
            }
            return string.Empty;
        }
        int INavigationControlTestable.GetGroupCount()
        {
            return ASPxMenuControl.Items.Count;
        }
        int INavigationControlTestable.GetSubGroupCount(ChoiceActionItem item)
        {
            DevExpress.Web.MenuItem group = FindMenuGroupControl(item);

            if (group != null && group.Items.Count > 0)
            {
                return group.Items[0].Items.Count;
            }
            return 0;
        }
        bool INavigationControlTestable.IsGroupExpanded(ChoiceActionItem item)
        {
            if (ActionItemToMenuGroupMap.ContainsKey(item))
            {
                return false;
            }
            return false;
        }
        string INavigationControlTestable.GetSelectedItemCaption()
        {
            if (ASPxMenuControl.SelectedItem != null)
            {
                return ASPxMenuControl.SelectedItem.Text;
            }

            return string.Empty;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            ASPxMenuControl.ItemClick -= ASPxMenuControl_ItemClick;
            ASPxMenuControl.Load -= ASPxMenuControl_Load;
            ClearItemWrappers();
            UnsubscribeAll();
            foreach (DevExpress.Web.MenuItem group in ASPxMenuControl.Items)
            {
                foreach (DevExpress.Web.MenuItem item in group.Items)
                {
                    if (item is IDisposable)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
                if (group is IDisposable)
                {
                    ((IDisposable)group).Dispose();
                }
            }
            RegisterCallbackStartupScript = null;
        }
        #endregion

        #region ISupportCallbackStartupScriptRegistering Members
        public event EventHandler<RegisterCallbackStartupScriptEventArgs> RegisterCallbackStartupScript;
        #endregion

        #region IXafCallbackHandler Members
        public void ProcessAction(string parameter)
        {
            if (SingleChoiceAction.Active && SingleChoiceAction.Enabled)
            {
                ChoiceActionItem item = SingleChoiceAction.FindItemByIdPath(parameter);
                if (item != null)
                {
                    SingleChoiceAction.DoExecute(item);
                }
            }
        }
        #endregion
    }
}
