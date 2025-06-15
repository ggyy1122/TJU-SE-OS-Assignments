#include "FloorButtonsWidget.h"
#include <QLabel>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8")// ��ָ���֧��VS����
#endif
FloorButtonsWidget::FloorButtonsWidget(QWidget* parent)
    : QWidget(parent)
{
    createFloorButtons();
}

FloorButtonsWidget::~FloorButtonsWidget() {}

void FloorButtonsWidget::createFloorButtons()
{
    QVBoxLayout* mainLayout = new QVBoxLayout(this);  // ����ֱ���֣����ϵ������ 20~1 ¥
    // ��ӱ���
    QLabel* titleLabel = new QLabel("      �ⲿ��ť", this);
    titleLabel->setAlignment(Qt::AlignCenter);
    titleLabel->setStyleSheet("font-size: 18px; font-weight: bold; margin-bottom: 10px;");  // ���ñ������ʽ
    mainLayout->addWidget(titleLabel);
    for (int i = 20; i >= 1; --i) {  // �������¥��
        QHBoxLayout* rowLayout = new QHBoxLayout();  // ÿһ���ˮƽ����

        // ������ǩ
        QLabel* floorLabel = new QLabel(QString::number(i) + " ¥", this);
        floorLabel->setFixedWidth(50);
        floorLabel->setAlignment(Qt::AlignRight | Qt::AlignVCenter);
        rowLayout->addWidget(floorLabel);

        // �������������ť��1¥û�У�
        if (i != 20) {
            QPushButton* upButton = new QPushButton("��", this);
            rowLayout->addWidget(upButton);
            connect(upButton, &QPushButton::clicked, this, [=]() {
                emit floorRequested(i, true);  // ����
                });
        }
        else {
            rowLayout->addSpacing(1);  // ռλ�����ְ�ť����
        }

        // ��������½���ť��20¥û�У�
        if (i != 1) {
            QPushButton* downButton = new QPushButton("��", this);
            rowLayout->addWidget(downButton);
            connect(downButton, &QPushButton::clicked, this, [=]() {
                emit floorRequested(i, false);  // �½�
                });
        }
        else {
            rowLayout->addSpacing(1);  // ռλ�����ְ�ť����
        }

        mainLayout->addLayout(rowLayout);
    }

    setLayout(mainLayout);
}



