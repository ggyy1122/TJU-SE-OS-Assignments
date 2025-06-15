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
    void floorRequested(int floor, bool isUp);  // 第二个参数 true 表示上升，false 表示下降


private:
    void createFloorButtons();
};

#endif // FLOORBUTTONSWIDGET_H
