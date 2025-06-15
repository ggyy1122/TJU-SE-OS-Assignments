#include "LogPanel.h"
#include <QVBoxLayout>
#include <QLabel>
#include <QFrame>
#include <QScrollBar>
#include <QTextCursor>

#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8") // 该指令仅支持VS环境
#endif

LogPanel::LogPanel(QWidget* parent)
    : QWidget(parent),
    logTextEdit(new QTextEdit(this))
{
    // 创建主布局
    QVBoxLayout* layout = new QVBoxLayout(this);

    // 添加标题
    QLabel* titleLabel = new QLabel("控制台", this);
    titleLabel->setStyleSheet("font-weight: bold; font-size: 32px; color: #333;");
    titleLabel->setAlignment(Qt::AlignCenter);

    // 创建带边框的框架容器
    QFrame* frame = new QFrame(this);
    frame->setFrameShape(QFrame::Box);
    frame->setLineWidth(2);
    frame->setStyleSheet("background-color: #C5C9C9;");

    // 布局：把 QTextEdit 放进框里
    QVBoxLayout* frameLayout = new QVBoxLayout(frame);
    frameLayout->addWidget(logTextEdit);
    frameLayout->setContentsMargins(5, 5, 5, 5);

    // 设置 QTextEdit 样式
    logTextEdit->setReadOnly(true);
    logTextEdit->setStyleSheet("border: none; font-family: Consolas, monospace;");

    // 启用自动换行
    logTextEdit->setLineWrapMode(QTextEdit::WidgetWidth);

    // 总是显示垂直滚动条
    logTextEdit->setVerticalScrollBarPolicy(Qt::ScrollBarAlwaysOn);

    // 确保内容足够大时可以滚动
    logTextEdit->setMinimumHeight(100); // 设置最小高度确保滚动条可见

    // 组合整体布局
    layout->addWidget(titleLabel);
    layout->addWidget(frame);

    setMinimumSize(300, 300);  // 设置最小窗口大小
}

void LogPanel::appendLog(const QString& text)
{
    // 追加日志
    logTextEdit->append(text);

    // 获取滚动条
    QScrollBar* scrollBar = logTextEdit->verticalScrollBar();
    if (scrollBar) {
        // 自动滚动到底部
        scrollBar->setValue(scrollBar->maximum());
    }
}