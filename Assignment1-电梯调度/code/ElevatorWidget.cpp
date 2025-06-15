#include "ElevatorWidget.h"
#include <QPainter>
#include <QVBoxLayout>
#include <QDebug>
#include <QGraphicsDropShadowEffect>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8")// 该指令仅支持VS环境
#endif

ElevatorWidget::ElevatorWidget(int totalFloors, QWidget* parent,int x)
    : QWidget(parent),
    totalFloors(totalFloors),
    currentFloor(1),
    floorHighlight(totalFloors, false),
    floorLabel(new QLabel(this)),
    start_x(x),
    doorOpen(false)
{
    // 初始状态电梯在1层
    floorHighlight[1] = true;

    // 不使用布局，手动设置位置
// 设置楼层标签位置和大小
    floorLabel->setGeometry(start_x-8, 15, 120, 45);
    floorLabel->setText("1");  // 只显示数字

    // 设置字体
    QFont font;
    font.setPointSize(20);       // 更大更醒目
    font.setBold(true);          // 加粗
    font.setFamily("Courier New");  // 等宽字体更有数码管味
    floorLabel->setFont(font);

    // 设置居中显示文本
    floorLabel->setAlignment(Qt::AlignCenter);

    // 设置示波管风格样式
    floorLabel->setStyleSheet(
        "QLabel {"
        "  background-color: white;"         // 白色背景
        "  color: #EE2E2E;"                  // 荧光绿字体
        "  border: 0.5px solid black;"         // 黑色边框
        "  border-radius: 0px;"              // 略有圆角
        "  padding: 6px;"                    // 内边距
        "}"
    );

    // 添加当前楼层标签
    QLabel* currentFloorLabel = new QLabel("当前楼层", this);
    currentFloorLabel->setGeometry(start_x - 25, 68, 155, 30);  // 设置位置
    currentFloorLabel->setFont(QFont("宋体", 12));  // 宋体，大小12
    currentFloorLabel->setAlignment(Qt::AlignCenter);  // 居中显示文本
    currentFloorLabel->setStyleSheet("QLabel { background-color: transparent; color: black; }");


    // 添加透明按钮用于点击每一层
    for (int i = 1; i < totalFloors; ++i) {
        QPushButton* button = new QPushButton(this);
        int startX = start_x;
        int startY = 300;
        int width = 100;
        int height = 30;
        int gap = 10;
        int y = startY + (totalFloors - 1 - i) * height - gap * (i - 1);
        button->setGeometry(startX, y, width, height);

        // 设置按钮为透明
        button->setStyleSheet("background: transparent; border: 2px solid red;");
        button->setCursor(Qt::PointingHandCursor);  // 可选，让鼠标变成小手

        // 设置按钮显示楼层文字
        button->setText(QString("%1 楼").arg(i));

        // 连接按钮点击信号
        connect(button, &QPushButton::clicked, this, [=]() {
            // 点击按钮时，按钮边框消失，背景变灰
            button->setStyleSheet("background: #C5C9C9; border: 2px solid  #C5C9C9; color: black;");
            emit floorRequested(i);  // 发出请求楼层的信号
            });

        buttons.push_back(button);  // 将按钮存储到按钮列表中
    }
}

ElevatorWidget::~ElevatorWidget() {}

void ElevatorWidget::updateFloorLabel(int floor)
{
    if (floor > 0 && floor < totalFloors) {
        floorHighlight.fill(false);
        floorHighlight[floor] = true;
        currentFloor = floor;

        floorLabel->setText(QString("%1").arg(floor));
        update();  // 触发重绘
    }
}

void ElevatorWidget::paintEvent(QPaintEvent* /*event*/)
{
    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    int startX = start_x;
    int startY = 300;
    int width = 100;
    int height = 30;
    int gap = 10;
    for (int i = 1; i < totalFloors; ++i) {
        int y = startY + (totalFloors - 1 - i) * height - gap * (i - 1);
        QRect rect(startX, y, width, height);

        // 背景色
        painter.setBrush(floorHighlight[i] ? Qt::green : Qt::white);
        // 使用透明画笔
        painter.setPen(QPen(Qt::transparent));  
        painter.drawRoundedRect(rect, 8, 8);
        // 显示楼层数字
        painter.setPen(Qt::black);
        painter.drawText(rect, Qt::AlignCenter, QString("%1 楼").arg(i));
    }

    // ---- 添加电梯门绘制（位于按钮下方） ----
    int doorX = start_x;
    int doorY = startY + totalFloors * height + 0.1; // 在按钮区域下方
    int doorWidth = width;
    int doorHeight = 40;

    QRect doorRect(doorX, doorY, doorWidth, doorHeight);

    // 设置门的颜色和文字
    if (doorOpen) {
        painter.setBrush(QColor("#A0FFA0"));  // 门开：绿色
        painter.setPen(Qt::darkGreen);
        painter.drawRoundedRect(doorRect, 6, 6);
        painter.drawText(doorRect, Qt::AlignCenter, "门已开");
    }
    else {
        painter.setBrush(QColor("#FFA0A0"));  // 门关：红色
        painter.setPen(Qt::darkRed);
        painter.drawRoundedRect(doorRect, 6, 6);
        painter.drawText(doorRect, Qt::AlignCenter, "门已关");
    }

}

// 电梯到达楼层后的恢复按钮状态
void ElevatorWidget::restoreButtonState(int floor)
{
    if (floor >= 1 && floor < totalFloors) {
        QPushButton* button = buttons[floor - 1];  // 获取相应楼层的按钮
        button->setStyleSheet("background: transparent; border: 2px solid red;");
    }
}

void ElevatorWidget::openDoor() {
    doorOpen = true;
    update(); // 触发重绘
}

void ElevatorWidget::closeDoor() {
    doorOpen = false;
    update(); // 触发重绘
}

