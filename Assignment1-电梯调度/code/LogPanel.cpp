#include "LogPanel.h"
#include <QVBoxLayout>
#include <QLabel>
#include <QFrame>
#include <QScrollBar>
#include <QTextCursor>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8") // ��ָ���֧��VS����
#endif

LogPanel::LogPanel(QWidget* parent)
    : QWidget(parent),
    logTextEdit(new QTextEdit(this))
{
    // ����������
    QVBoxLayout* layout = new QVBoxLayout(this);

    // ��ӱ���
    QLabel* titleLabel = new QLabel("����̨", this);
    titleLabel->setStyleSheet("font-weight: bold; font-size: 32px; color: #333;");
    titleLabel->setAlignment(Qt::AlignCenter);

    // �������߿�Ŀ������
    QFrame* frame = new QFrame(this);
    frame->setFrameShape(QFrame::Box);
    frame->setLineWidth(2);
    frame->setStyleSheet("background-color: #C5C9C9;");

    // ���֣��� QTextEdit �Ž�����
    QVBoxLayout* frameLayout = new QVBoxLayout(frame);
    frameLayout->addWidget(logTextEdit);
    frameLayout->setContentsMargins(5, 5, 5, 5);

    // ���� QTextEdit ��ʽ
    logTextEdit->setReadOnly(true);
    logTextEdit->setStyleSheet("border: none; font-family: Consolas, monospace;");

    // �����Զ�����
    logTextEdit->setLineWrapMode(QTextEdit::WidgetWidth);

    // ������ʾ��ֱ������
    logTextEdit->setVerticalScrollBarPolicy(Qt::ScrollBarAlwaysOn);

    // ȷ�������㹻��ʱ���Թ���
    logTextEdit->setMinimumHeight(100); // ������С�߶�ȷ���������ɼ�

    // ������岼��
    layout->addWidget(titleLabel);
    layout->addWidget(frame);

    setMinimumSize(300, 300);  // ������С���ڴ�С
}

void LogPanel::appendLog(const QString& text)
{
    // ׷����־
    logTextEdit->append(text);

    // ��ȡ������
    QScrollBar* scrollBar = logTextEdit->verticalScrollBar();
    if (scrollBar) {
        // �Զ��������ײ�
        scrollBar->setValue(scrollBar->maximum());
    }
}