#ifndef FLOORBUTTONSWIDGET_H
#define FLOORBUTTONSWIDGET_H

#include <QWidget>
#include <QPushButton>
#include <QVBoxLayout>

class FloorButtonsWidget : public QWidget
{
    Q_OBJECT

public:
    explicit FloorButtonsWidget(QWidget* parent = nullptr);
    ~FloorButtonsWidget();

signals:
    void floorRequested(int floor, bool isUp);  // �ڶ������� true ��ʾ������false ��ʾ�½�


private:
    void createFloorButtons();
};

#endif // FLOORBUTTONSWIDGET_H
