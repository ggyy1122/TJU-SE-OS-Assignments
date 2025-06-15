#ifndef LOGPANEL_H
#define LOGPANEL_H

#include <QWidget>
#include <QTextEdit>

class LogPanel : public QWidget
{
    Q_OBJECT
public:
    explicit LogPanel(QWidget* parent = nullptr);

public slots:
    void appendLog(const QString& text);  // 外部调用：添加日志

private:
    QTextEdit* logTextEdit;
};

#endif // LOGPANEL_H
