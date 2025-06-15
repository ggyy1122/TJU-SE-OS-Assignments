#include "ElevatorWidget.h"
#include <QPainter>
#include <QVBoxLayout>
#include <QDebug>
#include <QGraphicsDropShadowEffect>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8")// ��ָ���֧��VS����
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
    // ��ʼ״̬������1��
    floorHighlight[1] = true;

    // ��ʹ�ò��֣��ֶ�����λ��
// ����¥���ǩλ�úʹ�С
    floorLabel->setGeometry(start_x-8, 15, 120, 45);
    floorLabel->setText("1");  // ֻ��ʾ����

    // ��������
    QFont font;
    font.setPointSize(20);       // �������Ŀ
    font.setBold(true);          // �Ӵ�
    font.setFamily("Courier New");  // �ȿ�������������ζ
    floorLabel->setFont(font);

    // ���þ�����ʾ�ı�
    floorLabel->setAlignment(Qt::AlignCenter);

    // ����ʾ���ܷ����ʽ
    floorLabel->setStyleSheet(
        "QLabel {"
        "  background-color: white;"         // ��ɫ����
        "  color: #EE2E2E;"                  // ӫ��������
        "  border: 0.5px solid black;"         // ��ɫ�߿�
        "  border-radius: 0px;"              // ����Բ��
        "  padding: 6px;"                    // �ڱ߾�
        "}"
    );

    // ��ӵ�ǰ¥���ǩ
    QLabel* currentFloorLabel = new QLabel("��ǰ¥��", this);
    currentFloorLabel->setGeometry(start_x - 25, 68, 155, 30);  // ����λ��
    currentFloorLabel->setFont(QFont("����", 12));  // ���壬��С12
    currentFloorLabel->setAlignment(Qt::AlignCenter);  // ������ʾ�ı�
    currentFloorLabel->setStyleSheet("QLabel { background-color: transparent; color: black; }");


    // ���͸����ť���ڵ��ÿһ��
    for (int i = 1; i < totalFloors; ++i) {
        QPushButton* button = new QPushButton(this);
        int startX = start_x;
        int startY = 300;
        int width = 100;
        int height = 30;
        int gap = 10;
        int y = startY + (totalFloors - 1 - i) * height - gap * (i - 1);
        button->setGeometry(startX, y, width, height);

        // ���ð�ťΪ͸��
        button->setStyleSheet("background: transparent; border: 2px solid red;");
        button->setCursor(Qt::PointingHandCursor);  // ��ѡ���������С��

        // ���ð�ť��ʾ¥������
        button->setText(QString("%1 ¥").arg(i));

        // ���Ӱ�ť����ź�
        connect(button, &QPushButton::clicked, this, [=]() {
            // �����ťʱ����ť�߿���ʧ���������
            button->setStyleSheet("background: #C5C9C9; border: 2px solid  #C5C9C9; color: black;");
            emit floorRequested(i);  // ��������¥����ź�
            });

        buttons.push_back(button);  // ����ť�洢����ť�б���
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
        update();  // �����ػ�
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

        // ����ɫ
        painter.setBrush(floorHighlight[i] ? Qt::green : Qt::white);
        // ʹ��͸������
        painter.setPen(QPen(Qt::transparent));  
        painter.drawRoundedRect(rect, 8, 8);
        // ��ʾ¥������
        painter.setPen(Qt::black);
        painter.drawText(rect, Qt::AlignCenter, QString("%1 ¥").arg(i));
    }

    // ---- ��ӵ����Ż��ƣ�λ�ڰ�ť�·��� ----
    int doorX = start_x;
    int doorY = startY + totalFloors * height + 0.1; // �ڰ�ť�����·�
    int doorWidth = width;
    int doorHeight = 40;

    QRect doorRect(doorX, doorY, doorWidth, doorHeight);

    // �����ŵ���ɫ������
    if (doorOpen) {
        painter.setBrush(QColor("#A0FFA0"));  // �ſ�����ɫ
        painter.setPen(Qt::darkGreen);
        painter.drawRoundedRect(doorRect, 6, 6);
        painter.drawText(doorRect, Qt::AlignCenter, "���ѿ�");
    }
    else {
        painter.setBrush(QColor("#FFA0A0"));  // �Źأ���ɫ
        painter.setPen(Qt::darkRed);
        painter.drawRoundedRect(doorRect, 6, 6);
        painter.drawText(doorRect, Qt::AlignCenter, "���ѹ�");
    }

}

// ���ݵ���¥���Ļָ���ť״̬
void ElevatorWidget::restoreButtonState(int floor)
{
    if (floor >= 1 && floor < totalFloors) {
        QPushButton* button = buttons[floor - 1];  // ��ȡ��Ӧ¥��İ�ť
        button->setStyleSheet("background: transparent; border: 2px solid red;");
    }
}

void ElevatorWidget::openDoor() {
    doorOpen = true;
    update(); // �����ػ�
}

void ElevatorWidget::closeDoor() {
    doorOpen = false;
    update(); // �����ػ�
}

