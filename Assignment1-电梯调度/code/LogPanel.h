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
    void appendLog(const QString& text);  // �ⲿ���ã������־

private:
    QTextEdit* logTextEdit;
};

#endif // LOGPANEL_H
