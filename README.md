该项目是一个小型的，用于处理Unity中，YAML格式文件（如预制件、场景）出现的合并冲突问题的工具。

具体作用为：

在Git合并出现冲突时，分析YAML文件的内容，并区分Base，Remote和Local三个不同冲突文件中各自的修改，并将文件进行排序，以便差异工具能够正确比较差异。

# 准备工作

Git软件推荐使用[TortoiseGit](https://tortoisegit.org/)，差异比较工具推荐使用[KDiff3](https://kdiff3.sourceforge.net/)，其余工具均没有经过本人测试。

之后按照[说明](https://tortoisegit.org/docs/tortoisegit/tgit-dug-diff.html)，将KDiff3设置为TortoiseGit的默认差异比较工具。

如果设置成功，你在TortoiseGit中双击冲突文件时，应该就会打开KDiff3作为差异比较工具。

# 如何使用

## 解决文件冲突

1. 在Git中处理YAML格式的冲突文件时，打开KDiff3查看该文件的差异。在这个文件正处于打开状态时，该文件的位置会产生XXX.BASE，XXX.REMOTE，XXX.LOCAL这三个文件。
2. 之后打开该工具，输入Diff，回车。
3. 输入该文件的路径（或者直接将文件拖动到对话框中）。正确输入的文件名应为XXX，而非XXX.BASE，XXX.REMOTE，XXX.LOCAL等。
4. 单击回车开始对文件进行整理。
5. 整理成功后，在KDiff中按F5刷新文件，并选择“不保存并继续”（Continue without saving），之后文件内容会被刷新。
6. 新的文件内容被分成6个部分：
   - 内容相同的部分。
   - #modifies#：两个文件中都存在，但是内容不同的部分。
   - #remote adds#：Remote文件中添加的部分。
   - #local adds#：Local文件中添加的部分。
   - #remote deletes#：Remote文件中已经不存在的部分。
   - #local deletes#：Local文件中已经不存在的部分。
7. 之后就可以按照排序好的文件进行差异比较，并消除冲突。
8. 如果两个文件第6步中的部分标题没有对齐，可以使用KDiff3中的Add Manual Diff Alignment（手动添加差异对齐）功能，在这些标题的位置手动添加对齐点。
9. 冲突消除后，保存文件，并在Git中将文件标记为冲突已解决。

## 文件排序

如果你觉得按照上述方法整理过的文件相比之前的版本差异显得更多了，很乱，可以使用这个功能。

1. 保存一份你用于排序的依据YAML文件，这样就会按照该文件中的号码顺序来进行排序。
2. 打开该工具，输入Sort，回车。
3. 输入用于排序的依据文件的路径（或者直接将文件拖动到对话框中），回车。
4. 输入需要被排序的文件的路径（或者直接将文件拖动到对话框中），回车。
5. 排序完成。