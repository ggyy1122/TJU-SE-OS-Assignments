#include "FloorButtonsWidget.h"
#include <QLabel>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8")// 该指令仅支持VS环境
#endif
FloorButtonsWidget::FloorButtonsWidget(QWidget* parent)
    : QWidget(parent)
{
    createFloorButtons();
}

FloorButtonsWidget::~FloorButtonsWidget() {}

void FloorButtonsWidget::createFloorButtons()
{
    QVBoxLayout* mainLayout = new QVBoxLayout(this);  // 主垂直布局：从上到下添加 20~1 楼
    // 添加标题
    QLabel* titleLabel = new QLabel("      外部按钮", this);
    titleLabel->setAlignment(Qt::AlignCenter);
    titleLabel->setStyleSheet("font-size: 18px; font-weight: bold; margin-bottom: 10px;");  // 设置标题的样式
    mainLayout->addWidget(titleLabel);
    for (int i = 20; i >= 1; --i) {  // 倒序添加楼层
        QHBoxLayout* rowLayout = new QHBoxLayout();  // 每一层的水平布局

        // 层数标签
        QLabel* floorLabel = new QLabel(QString::number(i) + " 楼", this);
        floorLabel->setFixedWidth(50);
        floorLabel->setAlignment(Qt::AlignRight | Qt::AlignVCenter);
        rowLayout->addWidget(floorLabel);

        // 条件添加上升按钮（1楼没有）
        if (i != 20) {
            QPushButton* upButton = new QPushButton("↑", this);
            rowLayout->addWidget(upButton);
            connect(upButton, &QPushButton::clicked, this, [=]() {
                emit floorRequested(i, true);  // 上升
                });
        }
        else {
            rowLayout->addSpacing(1);  // 占位：保持按钮对齐
        }

        // 条件添加下降按钮（20楼没有）
        if (i != 1) {
            QPushButton* downButton = new QPushButton("↓", this);
            rowLayout->addWidget(downButton);
            connect(downButton, &QPushButton::clicked, this, [=]() {
                emit floorRequested(i, false);  // 下降
                });
        }
        else {
            rowLayout->addSpacing(1);  // 占位：保持按钮对齐
        }

        mainLayout->addLayout(rowLayout);
    }

    setLayout(mainLayout);
}



