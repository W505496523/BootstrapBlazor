﻿// **********************************
// 框架名称：BootstrapBlazor 
// 框架作者：Argo Zhang
// 开源地址：
// Gitee : https://gitee.com/LongbowEnterprise/BootstrapBlazor
// GitHub: https://github.com/ArgoZhang/BootstrapBlazor 
// 开源协议：LGPL-3.0 (https://gitee.com/LongbowEnterprise/BootstrapBlazor/blob/dev/LICENSE)
// **********************************

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    public partial class Table<TItem>
    {
        /// <summary>
        /// 获得/设置 是否显示工具栏 默认 false 不显示
        /// </summary>
        [Parameter]
        public bool ShowToolbar { get; set; }

        /// <summary>
        /// 获得/设置 是否显示加载骨架屏 默认 false 不显示
        /// </summary>
        [Parameter]
        public bool ShowSkeleton { get; set; }

        /// <summary>
        /// 获得/设置 是否显示按钮列 默认为 true
        /// </summary>
        /// <remarks>本属性设置为 true 新建编辑删除按钮设置为 false 可单独控制每个按钮是否显示</remarks>
        [Parameter]
        public bool ShowDefaultButtons { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示新建按钮 默认为 true 显示
        /// </summary>
        [Parameter]
        public bool ShowAddButton { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示编辑按钮 默认为 true 显示
        /// </summary>
        [Parameter]
        public bool ShowEditButton { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示删除按钮 默认为 true 显示
        /// </summary>
        [Parameter]
        public bool ShowDeleteButton { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示导出按钮 默认为 false 显示
        /// </summary>
        [Parameter]
        public bool ShowExportButton { get; set; }

        /// <summary>
        /// 获得/设置 是否显示扩展按钮 默认为 false
        /// </summary>
        [Parameter]
        public bool ShowExtendButtons { get; set; }

        /// <summary>
        /// 获得/设置 行内操作列宽度 默认为 130
        /// </summary>
        [Parameter]
        public int ExtendButtonColumnWidth { get; set; } = 130;

        /// <summary>
        /// 获得/设置 是否固定扩展按钮列 默认为 false 不固定
        /// </summary>
        [Parameter]
        public bool FixedExtendButtonsColumn { get; set; }

        /// <summary>
        /// 获得/设置 是否显示刷新按钮 默认为 true
        /// </summary>
        [Parameter]
        public bool ShowRefresh { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示列选择下拉框 默认为 false 不显示
        /// </summary>
        [Parameter]
        public bool ShowColumnList { get; set; }

        /// <summary>
        /// 获得/设置 表格 Toolbar 按钮模板
        /// </summary>
        [Parameter]
        public RenderFragment? TableToolbarTemplate { get; set; }

        /// <summary>
        /// 获得/设置 新建按钮回调方法
        /// </summary>
        [Parameter]
        public Func<Task<TItem>>? OnAddAsync { get; set; }

        /// <summary>
        /// 获得/设置 保存按钮异步回调方法
        /// </summary>
        [Parameter]
        public Func<TItem, Task<bool>>? OnSaveAsync { get; set; }

        /// <summary>
        /// 获得/设置 删除按钮异步回调方法
        /// </summary>
        [Parameter]
        public Func<IEnumerable<TItem>, Task<bool>>? OnDeleteAsync { get; set; }

        /// <summary>
        /// 获得/设置 导出按钮异步回调方法
        /// </summary>
        [Parameter]
        public Func<IEnumerable<TItem>, Task<bool>>? OnExportAsync { get; set; }

        /// <summary>
        /// 获得/设置 保存弹窗中的保存按钮显示文本 默认为资源文件中的 保存
        /// </summary>
        [Parameter]
        public string? EditDialogSaveButtonText { get; set; }

        /// <summary>
        /// ToastService 服务实例
        /// </summary>
        [Inject]
        [NotNull]
        protected ToastService? Toast { get; set; }

        /// <summary>
        /// DialogService 服务实例
        /// </summary>
        [Inject]
        [NotNull]
        protected DialogService? DialogService { get; set; }

        [Inject]
        [NotNull]
        private ITableExcelExport? ExcelExport { get; set; }

        /// <summary>
        /// 获得/设置 各列是否显示状态集合
        /// </summary>
        [NotNull]
        private IEnumerable<ColumnVisibleItem>? ColumnVisibles { get; set; }

        private class ColumnVisibleItem
        {
            [NotNull]
            public string? FieldName { get; set; }

            public bool Visible { get; set; }
        }

        private IEnumerable<ITableColumn> GetColumns()
        {
            var items = ColumnVisibles.Where(i => i.Visible);
            return Columns.Where(i => items.Any(v => v.FieldName == i.GetFieldName()));
        }

        private bool GetColumnsListState(ITableColumn col)
        {
            return ColumnVisibles.First(i => i.FieldName == col.GetFieldName()).Visible && ColumnVisibles.Count(i => i.Visible) == 1;
        }

        private bool ShowAddForm { get; set; }

        /// <summary>
        /// 新建按钮方法
        /// </summary>
        public async Task AddAsync()
        {
            if (OnSaveAsync != null)
            {
                if (OnAddAsync != null) EditModel = await OnAddAsync();
                else EditModel = new TItem();

                SelectedItems.Clear();
                EditModalTitleString = AddModalTitle;

                if (EditMode == EditMode.Popup)
                {
                    ShowEditorDialog();
                }
                else if (EditMode == EditMode.EditForm)
                {
                    ShowAddForm = true;
                    ShowEditForm = false;
                }
                StateHasChanged();
            }
            else
            {
                var option = new ToastOption
                {
                    Category = ToastCategory.Error,
                    Title = AddButtonToastTitle,
                    Content = AddButtonToastContent
                };
                Toast.Show(option);
            }
        }

        private bool ShowEditForm { get; set; }

        /// <summary>
        /// 编辑按钮方法
        /// </summary>
        public Task EditAsync()
        {
            if (OnSaveAsync != null)
            {
                if (SelectedItems.Count == 1)
                {
                    EditModel = SelectedItems[0].Clone();
                    EditModalTitleString = EditModalTitle;

                    if (EditMode == EditMode.Popup)
                    {
                        ShowEditorDialog();
                    }
                    else if (EditMode == EditMode.EditForm)
                    {
                        ShowEditForm = true;
                        ShowAddForm = false;
                    }
                }
                else
                {
                    var option = new ToastOption
                    {
                        Category = ToastCategory.Information,
                        Title = EditButtonToastTitle,
                        Content = SelectedItems.Count == 0 ? EditButtonToastNotSelectContent : EditButtonToastMoreSelectContent
                    };
                    Toast.Show(option);
                }
            }
            else
            {
                var option = new ToastOption
                {
                    Category = ToastCategory.Error,
                    Title = EditButtonToastTitle,
                    Content = EditButtonToastNoSaveMethodContent
                };
                Toast.Show(option);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 取消保存方法
        /// </summary>
        /// <returns></returns>
        protected EventCallback<MouseEventArgs> CancelSave() => EventCallback.Factory.Create<MouseEventArgs>(this, _ =>
        {
            if (EditMode == EditMode.EditForm)
            {
                ShowAddForm = false;
                ShowEditForm = false;
            }
        });

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="context"></param>
        protected async Task Save(EditContext context)
        {
            var valid = false;
            if (OnSaveAsync != null)
            {
                if (EditMode == EditMode.EditForm)
                {
                    ShowAddForm = false;
                    ShowEditForm = false;
                }

                valid = await OnSaveAsync((TItem)context.Model);
                var option = new ToastOption
                {
                    Category = valid ? ToastCategory.Success : ToastCategory.Error,
                    Title = SaveButtonToastTitle
                };
                option.Content = string.Format(SaveButtonToastResultContent, valid ? SuccessText : FailText, Math.Ceiling(option.Delay / 1000.0));
                Toast.Show(option);
                if (valid)
                {
                    DialogOption.Dialog?.Toggle();
                    await QueryAsync();
                }
            }
            else
            {
                var option = new ToastOption
                {
                    Category = ToastCategory.Error,
                    Title = SaveButtonToastTitle,
                    Content = SaveButtonToastContent
                };
                Toast.Show(option);
            }
        }

        private readonly DialogOption DialogOption = new DialogOption();

        /// <summary>
        /// 
        /// </summary>
        protected void ShowEditorDialog()
        {
            DialogOption.IsScrolling = ScrollingDialogContent;
            DialogOption.ShowFooter = false;
            DialogOption.Size = Size.ExtraLarge;
            DialogOption.Title = EditModalTitleString;

            var editorParameters = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.Model), EditModel),
                new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.Columns), Columns.Where(i => i.Editable)),
                new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.OnSaveAsync), new Func<EditContext, Task>(Save)),
                new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.ShowLabel), false),
                new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.BodyTemplate), EditTemplate!)
            };
            if (!string.IsNullOrEmpty(EditDialogSaveButtonText)) editorParameters.Add(new KeyValuePair<string, object>(nameof(TableEditorDialog<TItem>.SaveButtonText), EditDialogSaveButtonText));

            DialogOption.Component = DynamicComponent.CreateComponent<TableEditorDialog<TItem>>(editorParameters);
            DialogService.Show(DialogOption);
        }

        /// <summary>
        /// 确认删除按钮方法
        /// </summary>
        protected Task<bool> ConfirmDelete()
        {
            var ret = false;
            if (SelectedItems.Count == 0)
            {
                var option = new ToastOption
                {
                    Category = ToastCategory.Information,
                    Title = DeleteButtonToastTitle
                };
                option.Content = string.Format(DeleteButtonToastContent, Math.Ceiling(option.Delay / 1000.0));
                Toast.Show(option);
            }
            else
            {
                ret = true;
            }
            return Task.FromResult(ret);
        }

        /// <summary>
        /// 删除数据方法
        /// </summary>
        protected Func<Task> DeleteAsync() => async () =>
        {
            var ret = false;
            if (OnDeleteAsync != null) ret = await OnDeleteAsync(SelectedItems);
            var option = new ToastOption()
            {
                Title = DeleteButtonToastTitle
            };
            option.Category = ret ? ToastCategory.Success : ToastCategory.Error;
            option.Content = string.Format(DeleteButtonToastResultContent, ret ? SuccessText : FailText, Math.Ceiling(option.Delay / 1000.0));

            if (ret)
            {
                // 删除成功 重新查询
                // 由于数据删除导致页码会改变，尤其是最后一页
                // 重新计算页码
                // https://gitee.com/LongbowEnterprise/BootstrapBlazor/issues/I1UJSL
                PageIndex = Math.Max(1, Math.Min(PageIndex, int.Parse(Math.Ceiling((TotalCount - SelectedItems.Count) * 1d / PageItems).ToString())));
                var items = PageItemsSource.Where(item => item >= (TotalCount - SelectedItems.Count));
                PageItems = Math.Min(PageItems, items.Any() ? items.Min() : PageItems);

                SelectedItems.Clear();
                await QueryAsync();
            }
            Toast.Show(option);
        };

        /// <summary>
        /// 确认导出按钮方法
        /// </summary>
        protected Task<bool> ConfirmExport()
        {
            var ret = false;
            if (!Items.Any())
            {
                var option = new ToastOption
                {
                    Category = ToastCategory.Information,
                    Title = "导出数据"
                };
                option.Content = $"没有需要导出的数据, {Math.Ceiling(option.Delay / 1000.0)} 秒后自动关闭";
                Toast.Show(option);
            }
            else
            {
                ret = true;
            }
            return Task.FromResult(ret);
        }

        /// <summary>
        /// 导出数据方法
        /// </summary>
        protected async Task ExportAsync()
        {
            var ret = false;

            _ = Task.Run(async () =>
            {
                if (OnExportAsync != null)
                {
                    ret = await OnExportAsync(Items);
                }
                else
                {
                    // 如果未提供 OnExportAsync 回调委托使用注入服务来尝试解析
                    // TODO: 这里将本页数据作为参数传递给导出服务，服务本身可以利用自身优势获取全部所需数据，如果获取全部数据呢？
                    ret = await ExcelExport.ExportAsync(Items, Columns, JSRuntime);
                }

                var option = new ToastOption()
                {
                    Title = "导出数据"
                };
                option.Category = ret ? ToastCategory.Success : ToastCategory.Error;
                option.Content = $"导出数据{(ret ? "成功" : "失败")}, {Math.Ceiling(option.Delay / 1000.0)} 秒后自动关闭";

                Toast.Show(option);
            });

            var option = new ToastOption()
            {
                Title = "导出数据"
            };
            option.Category = ToastCategory.Information;
            option.Content = $"正在导出数据，请稍候, {Math.Ceiling(option.Delay / 1000.0)} 秒后自动关闭";

            Toast.Show(option);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取当前 Table 选中的所有行数据
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<TItem> GetSelectedRows() => SelectedItems;
    }
}
