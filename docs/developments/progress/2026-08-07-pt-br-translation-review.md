# AtomUI pt-BR 控件翻译审核

本文记录 AtomUI 首批巴西葡萄牙语公共控件翻译。当前基线包含 4 个模块语言包、13 个 XLIFF 文件和
68 个非 obsolete unit。

## 审核状态

- 初稿生成日期：2026-08-07。
- 状态提升日期：2026-08-07。
- 当前所有 target state 均为 `final`，满足静态语言包固定的 `final` 发布门禁。
- 本次状态提升由 AtomUI 维护者明确授权，依据项目内固定术语、权威英文 source、Ant Design `pt_BR` 参考和
  占位符契约进行 AI 辅助复核；没有巴西葡萄牙语母语译者背书。
- 状态提升只修改 target state，不修改 Catalog `file id`、unit Key、英文 source、占位符或模块身份。
- `Calendar.YearSuffix` 的 source 为空；巴西葡萄牙语不需要年份后缀，因此 target 是经过明确标记的合法空字符串。

## 术语与参考

主要参考：

- `.referenceprojects/ant-design/components/locale/pt_BR.ts`
- `.referenceprojects/ant-design/components/calendar/locale/pt_BR.ts`
- `.referenceprojects/ant-design/components/date-picker/locale/pt_BR.ts`
- `.referenceprojects/ant-design/components/time-picker/locale/pt_BR.ts`

稳定术语：

| English | pt-BR |
|---|---|
| Cancel | Cancelar |
| Delete | Excluir |
| Loading | Carregando |
| No data | Sem dados |
| Select all | Selecionar tudo |
| Previous | Anterior |
| Next | Próximo |
| Finish | Concluir |
| Save | Salvar |
| Reset | Redefinir |

## AtomUI.Controls.I18n.PtBR

Catalog: `AtomUI.Controls.Localization.CommonLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Cancel | Cancel | Cancelar | final |
| Delete | Delete | Excluir | final |
| Edit | Edit | Editar | final |
| Loading | Loading | Carregando | final |
| NoData | No data | Sem dados | final |
| Ok | Ok | OK | final |
| Optional | (optional) | (opcional) | final |
| Reset | Reset | Redefinir | final |
| Save | Save | Salvar | final |
| Submit | Submit | Enviar | final |

## AtomUI.Desktop.Controls.I18n.PtBR

### Calendar

Catalog: `AtomUI.Desktop.Controls.Localization.CalendarControlLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Month | Month | Mês | final |
| Week | Week | Semana | final |
| Year | Year | Ano | final |
| YearSuffix | `(empty string)` | `(empty string)` | final |

### DatePicker

Catalog: `AtomUI.Desktop.Controls.Localization.DatePickerLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Now | Now | Agora | final |
| Today | Today | Hoje | final |

### Dialog

Catalog: `AtomUI.Desktop.Controls.Localization.DialogLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Abort | Abort | Interromper | final |
| Apply | Apply | Aplicar | final |
| Cancel | Cancel | Cancelar | final |
| Close | Close | Fechar | final |
| Discard | Discard | Descartar | final |
| Help | Help | Ajuda | final |
| Ignore | Ignore | Ignorar | final |
| No | No | Não | final |
| NoToAll | No to All | Não para todos | final |
| Ok | OK | OK | final |
| Open | Open | Abrir | final |
| Reload | Reload | Recarregar | final |
| Reset | Reset | Redefinir | final |
| RestoreDefaults | Restore Defaults | Restaurar padrões | final |
| Retry | Retry | Tentar novamente | final |
| Save | Save | Salvar | final |
| SaveAll | Save All | Salvar tudo | final |
| Yes | Yes | Sim | final |
| YesToAll | Yes to All | Sim para todos | final |

### ImagePreviewer

Catalog: `AtomUI.Desktop.Controls.Localization.ImagePreviewerLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| ImageLoadFailed | Image load failed | Falha ao carregar a imagem | final |
| Preview | Preview | Visualizar | final |

### Pagination

Catalog: `AtomUI.Desktop.Controls.Localization.PaginationLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| JumpToText | Go to | Ir para | final |
| PageText | Page | Página | final |
| TotalInfoFormat | Total `${Total}` items | Total de `${Total}` itens | final |

### QRCode

Catalog: `AtomUI.Desktop.Controls.Localization.QRCodeLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Expired | QR code expired | Código QR expirado | final |
| Refresh | Refresh | Atualizar | final |
| Scanned | Scanned | Escaneado | final |

### TimePicker

Catalog: `AtomUI.Desktop.Controls.Localization.TimePickerLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| AMText | AM | AM | final |
| Now | Now | Agora | final |
| PMText | PM | PM | final |

### Tour

Catalog: `AtomUI.Desktop.Controls.Localization.TourLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| Finish | Finish | Concluir | final |
| Next | Next | Próximo | final |
| Previous | Previous | Anterior | final |

### Transfer

Catalog: `AtomUI.Desktop.Controls.Localization.TransferLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| DeSelectAll | deselect all data | Desmarcar tudo | final |
| InvertSelectCurrentPage | invert current page | Inverter seleção da página atual | final |
| Item | item | item | final |
| Items | items | itens | final |
| RemoveAll | remove all data | Remover tudo | final |
| RemoveCurrentPage | remove current page | Remover página atual | final |
| SelectAll | select all data | Selecionar tudo | final |
| SelectCurrentPage | select current page | Selecionar página atual | final |

### Upload

Catalog: `AtomUI.Desktop.Controls.Localization.UploadLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| DragUploadHead | Click or drag file to this area to upload | Clique ou arraste o arquivo para esta área para enviar | final |
| Pending | Pending... | Pendente... | final |
| Uploading | Uploading... | Enviando... | final |

## AtomUI.Desktop.Controls.DataGrid.I18n.PtBR

Catalog: `AtomUI.Desktop.Controls.DataGrid.Localization.DataGridLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| AscendTooltip | Click to sort ascending | Clique para ordenar em ordem crescente | final |
| CancelConfirmText | Sure to cancel? | Tem certeza de que deseja cancelar? | final |
| CancelTooltip | Click to cancel sorting | Clique para cancelar a ordenação | final |
| DeleteConfirmText | Sure to delete? | Tem certeza de que deseja excluir? | final |
| DescendTooltip | Click to sort descending | Clique para ordenar em ordem decrescente | final |
| Operating | Operation in progress, please wait. | Operação em andamento, aguarde. | final |
| SelectAllFilterItems | Select all items | Selecionar todos os itens | final |

## AtomUI.Desktop.Controls.ColorPicker.I18n.PtBR

Catalog: `AtomUI.Desktop.Controls.ColorPicker.Localization.ColorPickerLangResourceKind`

| Key | English source | Portuguese target | State |
|---|---|---|---|
| EmptyColorText | Transparent | Transparente | final |

## Aprovação

Autorização: mantenedor do AtomUI

Data da autorização: `2026-08-07`

Método de revisão: revisão assistida por IA da terminologia, do texto-fonte e dos contratos de placeholder

Resultado: promovido para `final` por autorização explícita do mantenedor

Atestação de falante nativo: não realizada. Esta seção registra com precisão a procedência da revisão e não representa uma
aprovação humana por um revisor nativo de português brasileiro.
